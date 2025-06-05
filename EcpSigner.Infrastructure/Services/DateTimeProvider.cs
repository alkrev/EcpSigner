using EcpSigner.Domain.Interfaces;
using System;

namespace EcpSigner.Infrastructure.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
