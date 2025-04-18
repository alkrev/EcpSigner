﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Portal;
using Cryptography;
using System.Threading;
using CacheTools;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EcpSigner
{
    internal class Signer
    {
        private string[] args;
        public Signer(string[] args)
        {
            this.args = args;
        }
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
        public async Task Run()
        {
            string ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            string title = $"EcpSigner v{ver}";
            logger.Info(title);
            Console.Title = title;

            // Устанавливаем рабочую папку
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = path;

            cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
            Console.CancelKeyPress += CancelKeyPressHandler;
            try
            {
                Settings s = GetSettings();
                await MainLoop(args, s, token);
            }
            catch (Exception ex)
            {
                logger.Fatal($"{ex.Message ?? "DoWork: фатальная ошибка"}");
            }
            logger.Info("работа завершена");
        }
        /**
         * Получаем настройки
         */
        private Settings GetSettings()
        {
            Settings s = Settings.Read("config.json");
            CheckSettings(s);
            s.ignoreDocTypesDict = s.ignoreDocTypes.ToDictionary(x => x, x => (byte)1);
            return s;
        }
        /** 
        * Проверка настроек
        */
        public void CheckSettings(Settings s)
        {
            if (string.IsNullOrEmpty(s.login))
            {
                throw new Exception("login пользователя не задан");
            }
            if (string.IsNullOrEmpty(s.password))
            {
                throw new Exception("password пользователя не задан");
            }
            if (string.IsNullOrEmpty(s.url))
            {
                throw new Exception("url сайта ЕЦП не задан");
            }
            if (s.pauseMinutes < 1 || s.pauseMinutes > 7 * 60 * 24)
            {
                s.pauseMinutes = 15;
                logger.Warn($"pauseMinutes задан некорректно. Установлено pauseMinutes={s.pauseMinutes}");
            }
            if (s.cacheMinutes < 1)
            {
                s.cacheMinutes = 360;
                logger.Warn($"cacheMinutes задан некорректно. Установлено cacheMinutes={s.cacheMinutes}");
            }
            if (s.signingIntervalSeconds < 1 || s.signingIntervalSeconds > 60)
            {
                s.signingIntervalSeconds = 1;
                logger.Warn($"signingIntervalSeconds задан некорректно. Установлено signingIntervalSeconds={s.signingIntervalSeconds}");
            }
        }
        /** 
         * Основной цикл
         */
        private async Task MainLoop(string[] args, Settings s, CancellationToken token)
        {
            Web.Client wc = new Web.Client(s.url);
            Cache c = new Cache(s.cacheMinutes);
            Portal p = new Portal(wc, c);
            DateTime lastCheck = DateTime.Now;
            while (true)
            {
                try
                {
                    cacheRemoveExpired(ref lastCheck, c);
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
                    logger.Warn("вход не выполнен");
                    p.isLoggedOn = false;
                }
                catch (Exception ex)
                {
                    logger.Error($"{ex.Message ?? "MainLoop: ошибка"}");
                }
                for (int i=0; i<s.pauseMinutes*60; i++)
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
         * Удаляем просроченные документы из кеша
         */
        private void cacheRemoveExpired(ref DateTime lastCheck, Cache c)
        {
            DateTime now = DateTime.Now;
            if (lastCheck.Day != now.Day)
            {
                logger.Info("очищаем кеш");
                c.RemoveExpired();
                lastCheck = now;
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
            (int, List<string>) result = await SignDocs(p, docs, certs, s, token);
            int count = result.Item1; // Количество успешно подписанных документов
            p.cache.SetRange(result.Item2); // Кешируем документы, которые возвращают ошибку при подписании
            
            stopTime = DateTime.UtcNow;
            logger.Info(string.Format("подписано документов [{0}] за {1:f} секунд. Кеш: {2}", count, (stopTime - startTime).TotalSeconds, p.cache.Count()));
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
                    logger.Info("'{0} - {1} ({2})': {3}", doc.Document_Name, doc.Document_Num, doc.EMDVersion_VersionNum, doc.Error_Msg);
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
        * Подписываем документы
        */
        private async Task<(int, List<string>)> SignDocs(Portal p, List<loadEMDSignBundleWindowReply> docs, List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> certs, Settings s, CancellationToken token)
        {
            int count = 0;
            List<string> errorDocNums = new List<string>();
            foreach (loadEMDSignBundleWindowReply doc in docs)
            {
                string document = string.Format("'{0} - {1} ({2})'", doc.Document_Name, doc.Document_Num, doc.EMDVersion_VersionNum);
                try
                {
                    if (token.IsCancellationRequested) throw new StopWorkException();

                    (loadEMDCertificateListReply ecpCert, CAPICOM.ICertificate userCert) = SelectCertificate(certs);
                    logger.Debug(string.Format("сертификат выбран: {0} срок действия {1}", userCert.SubjectName, userCert.ValidToDate.ToString("dd.MM.yyyy HH:mm:ss")));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    await CheckBeforeSign(p, doc, ecpCert);
                    logger.Debug(string.Format("проверка перед подписанием документа {0} прошла успешно", document));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    (string docBase64, string hashBase64) = await GetEMDVersionSignData(p, doc, ecpCert);
                    logger.Debug(string.Format("получение документа для подписания {0} прошло успешно", document));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    string signature = Crypto.Sign(userCert, docBase64);
                    logger.Debug(string.Format("подпись {0} создана", document));

                    if (token.IsCancellationRequested) throw new StopWorkException();

                    string EMDSignatureid = await SaveEMDSignature(p, doc, ecpCert, signature, hashBase64);
                    logger.Debug(string.Format("подпись документа {0} сохранена на сервере", document));

                    count++;
                }
                catch (WarningException ex)
                {
                    logger.Warn($"{document}: {ex.Message ?? "SignDocs: warning"}");
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
                    logger.Error($"{document}: {ex.Message ?? "SignDocs: ошибка"}");
                    break;
                }
                for (int i = 0; i < s.signingIntervalSeconds; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(1 * 1000);
                }
            }
            return (count, errorDocNums);
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
        private (loadEMDCertificateListReply ecpCert, CAPICOM.ICertificate userCert) SelectCertificate(List<Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate>> certs)
        {
            DateTime now = DateTime.Now;
            foreach (Tuple<loadEMDCertificateListReply, CAPICOM.ICertificate> cert in certs)
            {
                if (now + TimeSpan.FromMilliseconds(600) < cert.Item2.ValidToDate)
                {
                    return (cert.Item1, cert.Item2);
                }
                else
                {
                    logger.Warn(string.Format("сертификат невалидный: {0} срок действия {1}", cert.Item2.SubjectName, cert.Item2.ValidToDate.ToString("dd.MM.yyyy HH:mm:ss")));
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