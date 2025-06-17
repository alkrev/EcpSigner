using EcpSigner.Domain.Interfaces;
using System;

namespace EcpSigner.Infrastructure.Services
{
    public class DatesService : IDatesService
    {
        private readonly string[] args;
        private readonly IDateTimeProvider _dateTimeProvider;
        public DatesService(string[] _args, IDateTimeProvider dateTimeProvider)
        {
            args = _args;
            _dateTimeProvider = dateTimeProvider;
        }
        /**
        * Определяем диапазон дат в зависимости от параметров командной строки
        */
        public (string startDate, string endDate) GetDates()
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
        /**
        * Определяем диапазон дат
        */
        private (string startDate, string endDate) GetStartEndDates()
        {
            DateTime dateTime = _dateTimeProvider.Now;
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
                    startMonth = month - 1;
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
    }
}
