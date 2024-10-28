using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Portal;
using Cryptography;
using System.Linq;
using System.Threading;
using CacheTools;
using System.Diagnostics;
using System.IO;

namespace EcpSigner
{
    internal class Signer
    {
        /** 
        * Инициализируем логгер
        */
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /** 
        * Обрабатываем Ctrl-C
        */
        CancellationTokenSource cancelTokenSource;
        private void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e) {
            e.Cancel = true;
            cancelTokenSource.Cancel();
        }
        /** 
        * Главная функция программы
        */
        public async Task DoWork(string[] args)
        {
            string ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            logger.Info($"EcpSigner v{ver}");

            // Устанавливаем рабочую папку
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = path;

            cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
            Console.CancelKeyPress += CancelKeyPressHandler;
            try
            {
                Settings s = Settings.Read("config.json");
                s.CheckSettings(logger);
                await MainLoop(args, s, token);
            }
            catch (Exception ex)
            {
                logger.Fatal($"{ex.Message ?? "DoWork: фатальная ошибка"}");
            }
            logger.Info("работа завершена");
        }
        /** 
         * Основной цикл
         */
        private async Task MainLoop(string[] args, Settings s, CancellationToken token)
        {
            Web.Client wc = new Web.Client(s.url);
            Cache c = new Cache(s.cacheMinutes);
            Portal p = new Portal(wc, c);
            while (true)
            {
                try
                {
                    await signDocuments(args, p, s, token);
                }
                catch (BreakWorkException ex)
                {
                    logger.Fatal($"{ex.Message ?? "MainLoop: фатальная ошибка"}");
                    break;
                }
                catch (StopWorkException) {
                    logger.Info("остановка работы");
                    break;
                }
                catch (IsNotLoggedInException)
                {
                    logger.Warn($"{"вход не выполнен"}");
                    p.isLoggedOn = false;
                }
                catch (Exception ex)
                {
                    logger.Error($"{ex.Message ?? "MainLoop: ошибка"}");
                }
                for (int i=0; i<60; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(1 * 1000);
                }
            }
        }
        /**
         * Получаем и подписываем документы
         */
        private async Task signDocuments(string[] args, Portal p, Settings s, CancellationToken token)
        {
            (string startDate, string endDate) = GetDates(args);

            DateTime startTime;
            DateTime stopTime;

            if (token.IsCancellationRequested) throw new StopWorkException();

            if (!p.isLoggedOn)
            {
                logger.Info("выполняем вход");
                await Login(p, s.login, s.password);
                p.isLoggedOn = true;
                logger.Info("вход выполнен");
            }

            if (token.IsCancellationRequested) throw new StopWorkException();

            logger.Info($"получаем список документов {startDate}-{endDate}");
            startTime = DateTime.UtcNow;
            List<loadEMDSignBundleWindowReply> sdocs = await SearchDocuments(p, startDate, endDate, token);
            stopTime = DateTime.UtcNow;
            logger.Info(string.Format("получено документов {0} за {1:f} секунд", sdocs.Count, (stopTime - startTime).TotalSeconds));

            showDocWithErrors(sdocs.FindAll(x => x.IsSigned == "2")); // Показываем документы с ошибками

            // Убираем документы, которые не будем пытаться подписывать
            List<loadEMDSignBundleWindowReply> docsToSign = sdocs.FindAll(x => x.IsSigned == "2" && string.IsNullOrEmpty(x.Error_Msg)); // Оставляем где требуется подпись и нет ошибок
            List<loadEMDSignBundleWindowReply> rdocs = removeIgnoredDocs(s, docsToSign); // Убираем подписываемые вручную
            List<loadEMDSignBundleWindowReply> docs = rdocs.FindAll(x => !p.cache.Contains(x.EMDRegistry_ObjectID)); // Убираем кешированные
            logger.Info(string.Format("выбрано для отправки документов {0}", docs.Count));
            if (docs.Count == 0) return;

            if (token.IsCancellationRequested) throw new StopWorkException();

            logger.Info("получаем список сертификатов");
            startTime = DateTime.UtcNow;
            List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> certs = await GetCertificates(p);
            stopTime = DateTime.UtcNow;
            logger.Info(string.Format("получено сертификатов {0} за {1:f} секунд", certs.Count, (stopTime - startTime).TotalSeconds));

            if (token.IsCancellationRequested) throw new StopWorkException();

            logger.Info("подписываем документы");
            startTime = DateTime.UtcNow;
            Tuple<int, List<string>>[] results = await SignDocsSimultaneously(p, docs, certs, s.threadCount, token);
            int count = results.Select(x => x.Item1).Sum(); // Количество успешно подписанных документов
            foreach (Tuple<int, List<string>> result in results)
            {
                p.cache.SetRange(result.Item2); // Кешируем документы, которые возвращают ошибку при подписании
            }
            stopTime = DateTime.UtcNow;
            logger.Info(string.Format("подписано документов {0} за {1:f} секунд " + string.Join("", results.Select(x => $"[{x.Item1}]")) + " Кеш: {2}", count, (stopTime - startTime).TotalSeconds, p.cache.Count()));
        }
        /**
         * Убираем документы подписываемые вручную
         */
        private List<loadEMDSignBundleWindowReply> removeIgnoredDocs(Settings s, List<loadEMDSignBundleWindowReply> sdocs)
        {
            List<loadEMDSignBundleWindowReply> rdocs = new List<loadEMDSignBundleWindowReply>();
            bool needFlash = false;
            foreach (loadEMDSignBundleWindowReply doc in sdocs)
            {
                if (s.ignoreDocTypesDict.ContainsKey(doc.EMDRegistry_ObjectName))
                {
                    logger.Warn("'{0} - {1} ({2})' следует подписывать вручную", doc.Document_Name, doc.Document_Num, doc.EMDVersion_VersionNum);
                    needFlash = true;
                }
                else
                {
                    rdocs.Add(doc);
                }
            }
            if (needFlash)
            {
                StartFlashWindow();
            }
            else
            {
                StopFlashWindow();
            }
            return rdocs;
        }
        /**
         * Показываем документы с ошибками
         */
        private void showDocWithErrors(List<loadEMDSignBundleWindowReply> sdocs)
        {
            foreach (loadEMDSignBundleWindowReply doc in sdocs)
            {
                if (!string.IsNullOrEmpty(doc.Error_Msg))
                {
                    logger.Info("'{0} - {1} ({2}): {3}'", doc.Document_Name, doc.Document_Num, doc.EMDVersion_VersionNum, doc.Error_Msg);
                }
            }
            return;
        }
        /**
         * Мигаем иконкой в на панели задач
         */
        private void StartFlashWindow()
        {
            EcpSigner.FlashWindow.Start(Process.GetCurrentProcess().MainWindowHandle);
        }
        /**
         * Останавливаем мигание иконкой
         */
        private void StopFlashWindow()
        {
            EcpSigner.FlashWindow.Stop(Process.GetCurrentProcess().MainWindowHandle);
        }
        /**
        * Выполняем вход
        */
        private async Task Login(Portal p, string login, string password)
        {
            loginReply rep = await p.main.Login(login, password);
            if (!rep.success)
            {
                string err = rep.Error_Msg;
                throw new BreakWorkException(err);
            }
        }
        /**
        * Подписываем документы в несколько потоков
        */
        private async Task<Tuple<int, List<string>>[]> SignDocsSimultaneously(Portal p, List<loadEMDSignBundleWindowReply> docs, List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> certs, int threads, CancellationToken token)
        {
            if (threads <= 0)
            {
                threads = 1;
            }
            List<Task<Tuple<int, List<string>>>> tasks = new List<Task<Tuple<int, List<string>>>>(threads);
            for (int i=0; i<threads; i++)
            {
                List<loadEMDSignBundleWindowReply> taskDocs = docs.Where((x, j) => j % threads == i).ToList();
                int threadNum = i;
                tasks.Add(SignDocs(p, taskDocs, certs, threadNum, token));
            }
            Tuple<int, List<string>>[] results = new Tuple<int, List<string>>[0];
            Task<Tuple<int, List<string>>[]> task = Task.WhenAll(tasks);
            try
            {
                results = await task;
            }
            catch (Exception e)
            {
                if (task.Exception != null && task.Exception.InnerExceptions != null)
                {
                    foreach (Exception ex in task.Exception.InnerExceptions)
                    {
                        if (ex is IsNotLoggedInException)
                        {
                            throw ex;
                        }
                        if (ex is StopWorkException)
                        {
                            throw ex;
                        }
                    }
                }
                throw e;
            }
            return results;
        }
        /**
        * Подписываем документы
        */
        private async Task<Tuple<int, List<string>>> SignDocs(Portal p, List<loadEMDSignBundleWindowReply> docs, List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> certs, int t, CancellationToken token)
        {
            int count = 0;
            List<string> errorDocNums = new List<string>();
            foreach (loadEMDSignBundleWindowReply doc in docs)
            {
                string document = string.Format("'{0} - {1} ({2})'", doc.Document_Name, doc.Document_Num, doc.EMDVersion_VersionNum);
                try
                {
                    if (token.IsCancellationRequested) throw new StopWorkException();

                    (loadEMDCertificateListReply ecpCert, CAPICOM.ICertificate userCert) = SelectCertificate(certs, t);
                    logger.Debug(string.Format("[{0}] сертификат выбран: {1} срок действия {2}", t, userCert.SubjectName, userCert.ValidToDate.ToString("dd.MM.yyyy HH:mm:ss")));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    await CheckBeforeSign(p, doc, ecpCert);
                    logger.Debug(string.Format("[{0}] проверка перед подписанием документа {1} прошла успешно", t, document));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    (string docBase64, string hashBase64) = await GetEMDVersionSignData(p, doc, ecpCert);
                    logger.Debug(string.Format("[{0}] получение документа для подписания {1} прошло успешно", t, document));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    string signature = Crypto.Sign(userCert, docBase64);
                    logger.Debug(string.Format("[{0}] подпись {1} создана", t, document));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    string EMDSignatureid = await SaveEMDSignature(p, doc, ecpCert, signature, hashBase64);
                    logger.Debug(string.Format("[{0}] подпись документа {1} сохранена на сервере", t, document));

                    count++;
                }
                catch (WarningException ex)
                {
                    logger.Warn($"[{t}]{document}: {ex.Message ?? "SignDocs: warning"}");
                    errorDocNums.Add(doc.EMDRegistry_ObjectID);
                }
                catch (IsNotLoggedInException)
                {
                    throw;
                }
                catch (StopWorkException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error($"[{t}]{document}: {ex.Message ?? "SignDocs: ошибка"}");
                    break;
                }
            }
            return new Tuple<int, List<string>> (count, errorDocNums);
        }
        /**
        * Сохранение подписи документа
        */
        private async Task<string> SaveEMDSignature(Portal p, loadEMDSignBundleWindowReply doc, loadEMDCertificateListReply ecpCert, string signature, string docHash)
        {
            saveEMDSignaturesReply rep;
            try
            {
                rep = await p.emd.saveEMDSignatures(doc.EMDRegistry_ObjectName, doc.EMDRegistry_ObjectID, doc.EMDVersion_id, docHash, signature, ecpCert.EMDCertificate_id);
                if (!rep.success)
                {
                    string err = rep.Error_Msg;
                    throw new WarningException(err);
                }
            }
            catch (NotLoggedInException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
            return rep.EMDSignatures_id;
        }
        /**
        * Получение документа для подписания
        */
        private async Task<(string docBase64, string hashBase64)> GetEMDVersionSignData(Portal p, loadEMDSignBundleWindowReply doc, loadEMDCertificateListReply ecpCert)
        {
            getEMDVersionSignDataReply rep;
            try
            {
                rep = await p.emd.getEMDVersionSignData(doc.EMDRegistry_ObjectName, doc.EMDRegistry_ObjectID, ecpCert.EMDCertificate_id, doc.EMDVersion_VersionNum);
                if (!rep.success)
                {
                    string err = rep.Error_Msg;
                    throw new WarningException(err);
                }
                if (rep.toSign.Length == 0)
                {
                    throw new WarningException("GetEMDVersionSignData: toSign.Length = 0");
                }
            }
            catch (NotLoggedInException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
            return (rep.toSign[0].docBase64, rep.toSign[0].hashBase64);
        }
        /**
        * Проверка документа перед подписанием
        */
        private async Task CheckBeforeSign(Portal p, loadEMDSignBundleWindowReply doc, loadEMDCertificateListReply ecpCert)
        {
            try
            {
                checkBeforeSignReply rep = await p.emd.checkBeforeSign(doc.EMDRegistry_ObjectName, doc.EMDRegistry_ObjectID, ecpCert.EMDCertificate_id, doc.EMDVersion_id);
                if (!rep.success)
                {
                    string err = rep.Error_Msg;
                    throw new WarningException(err);
                }
            }
            catch (NotLoggedInException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
        }
        /**
        * Проверяем сертификаты
        */
        private (loadEMDCertificateListReply ecpCert, CAPICOM.ICertificate userCert) SelectCertificate(List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> certs, int t)
        {
            DateTime now = DateTime.Now;
            foreach (Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate> cert in certs)
            {
                if (now + TimeSpan.FromMilliseconds(200) < cert.Item2.ValidToDate)
                {
                    return (cert.Item1, cert.Item2);
                }
                else
                {
                    logger.Warn(string.Format("[{0}]сертификат невалидный: {1} срок действия {2}", t, cert.Item2.SubjectName, cert.Item2.ValidToDate.ToString("dd.MM.yyyy HH:mm:ss")));
                }
            }
            throw new Exception("подходящие сертификаты не найдены");
        }
        /**
        * Получаем сертификаты пользователя из ECP
        */
        private async Task<List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>>> GetCertificates(Portal p)
        {
            List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> suitableCerts;
            try
            {
                List<loadEMDCertificateListReply> certs = await p.emd.loadEMDCertificateList();
                if (certs.Count == 0)
                {
                    throw new BreakWorkException("У пользователя ECP не обнаружены сертификаты");
                }
                Dictionary<string, CAPICOM.ICertificate> userCerts = Crypto.GetUserCertificates();
                List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> foundCerts = new List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>>();
                foreach (loadEMDCertificateListReply cert in certs)
                {
                    string thumbprint = cert.EMDCertificate_SHA1.Replace("0x", "00").ToUpper();
                    if (userCerts.ContainsKey(thumbprint))
                    {
                        foundCerts.Add(new Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>(cert, userCerts[thumbprint]));
                    }
                }
                if (foundCerts.Count == 0)
                {
                    throw new BreakWorkException("У пользователя в операционной системе сертификаты не найдены");
                }
                suitableCerts = new List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>>();
                foreach (Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate> cert in foundCerts)
                {
                    try
                    {
                        Crypto.Sign(cert.Item2, "test");
                        suitableCerts.Add(cert);
                    }
                    catch { }                    
                }
                if (suitableCerts.Count == 0)
                {
                    throw new BreakWorkException("У пользователя в операционной системе подходящие сертификаты не найдены");
                }
            }
            catch (NotLoggedInException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
            return suitableCerts;
        }
        /**
        * Получаем список документов
        */
        private async Task<List<loadEMDSignBundleWindowReply>> SearchDocuments(Portal p, string startDate, string endDate, CancellationToken token)
        {
            List<loadEMDSignBundleWindowReply> docs;
            try
            {
                docs = new List<loadEMDSignBundleWindowReply>();
                int start = 0;
                int count = 30;
                int page = 1;
                while (true)
                {
                    if (token.IsCancellationRequested) throw new StopWorkException();
                    List<loadEMDSignBundleWindowReply> rep = await p.emd.loadEMDSignBundleWindow(startDate, endDate, start, page, count);
                    if (rep.Count == 0)
                    {
                        break;
                    }
                    docs.AddRange(rep);
                    start += count;
                    page += 1;
                }
            }
            catch (NotLoggedInException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
            return docs;
        }
        /**
        * Определяем диапазон дат
        */
        private (string startDate, string endDate) GetStartEndDates()
        {
            DateTime dateTime = DateTime.Now;
            string endDate = dateTime.ToString("dd.MM.yyyy");
            string startDate;
            int year = dateTime.Year;
            int month = dateTime.Month;
            int day = dateTime.Day;
            int startMonth;
            try
            {
                if (month == 1)
                {
                    startMonth = 12;
                    year--;
                }
                else
                {
                    startMonth = month-1;
                }
                DateTime start = new DateTime(year, startMonth, day);
                startDate = start.ToString("dd.MM.yyyy");
            }
            catch
            {
                DateTime start = new DateTime(year, month, 1);
                startDate = start.AddDays(-1).ToString("dd.MM.yyyy");
            }
            return (startDate, endDate);
        }
        /**
        * Определяем диапазон дат в зависимости от параметров командной строки
        */
        private (string startDate, string endDate) GetDates(string[] args)
        {
            string startDate;
            string endDate;
            if (args.Length == 1)
            {
                startDate = endDate = args[0];
            }
            else if (args.Length == 2)
            {
                startDate = args[0];
                endDate = args[1];
            }
            else
            {
                (startDate, endDate) = GetStartEndDates();
            }
            return (startDate, endDate);
        }
    }
    public class BreakWorkException : Exception
    {
        public BreakWorkException(string message) : base(message)
        {

        }
    }
    public class WarningException : Exception
    {
        public WarningException(string message) : base(message)
        {

        }
    }
    public class IsNotLoggedInException : Exception
    {
        public IsNotLoggedInException(string message) : base(message)
        {

        }
    }
    public class StopWorkException : Exception { }
}