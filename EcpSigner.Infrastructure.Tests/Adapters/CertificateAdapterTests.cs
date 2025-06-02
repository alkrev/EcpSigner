using System;
using Xunit;
using FluentAssertions;
using CAPICOM;

namespace EcpSigner.Infrastructure.Adapters
{
    public class CertificateAdapterTests
    {
        // Фейковая реализация CAPICOM.ICertificate
        private class FakeCertificate : CAPICOM.ICertificate
        {
            public string SubjectName { get; set; }
            public DateTime ValidToDate { get; set; }

            // Остальные члены интерфейса можно не реализовывать, если они не используются
            public string IssuerName => throw new NotImplementedException();
            public DateTime ValidFromDate => throw new NotImplementedException();
            public string SerialNumber => throw new NotImplementedException();
            public string Thumbprint => throw new NotImplementedException();
            public string Version => throw new NotImplementedException();
            public object Extensions => throw new NotImplementedException();
            public object PublicKey => throw new NotImplementedException();
            public object PrivateKey => throw new NotImplementedException();
            public void Display() => throw new NotImplementedException();
            public void Save(string fileName, string password, CAPICOM.CAPICOM_ENCODING_TYPE encodingType) => throw new NotImplementedException();
            public string Export(CAPICOM.CAPICOM_ENCODING_TYPE encodingType) => throw new NotImplementedException();
            public bool HasPrivateKey() => throw new NotImplementedException();
            public string GetInfo(CAPICOM_CERT_INFO_TYPE InfoType) => throw new NotImplementedException();
            public ICertificateStatus IsValid() => throw new NotImplementedException();
            public KeyUsage KeyUsage() => throw new NotImplementedException();
            public ExtendedKeyUsage ExtendedKeyUsage() => throw new NotImplementedException();
            public BasicConstraints BasicConstraints() => throw new NotImplementedException();
            public void Import(string EncodedCertificate) => throw new NotImplementedException();
            int ICertificate.Version => throw new NotImplementedException();
        }

        [Fact]
        public void Subject_Should_Return_Certificate_SubjectName()
        {
            // Arrange
            var fakeCert = new FakeCertificate { SubjectName = "CN=Test User" };
            var adapter = new CertificateAdapter(fakeCert);

            // Act
            var subject = adapter.Subject;

            // Assert
            subject.Should().Be("CN=Test User");
        }

        [Fact]
        public void ValidToDate_Should_Return_Certificate_ValidToDate()
        {
            // Arrange
            var expectedDate = new DateTime(2030, 12, 31);
            var fakeCert = new FakeCertificate { ValidToDate = expectedDate };
            var adapter = new CertificateAdapter(fakeCert);

            // Act
            var validTo = adapter.ValidToDate;

            // Assert
            validTo.Should().Be(expectedDate);
        }

        [Fact]
        public void GetCertificate_Should_Return_Original_Com_Certificate()
        {
            // Arrange
            var fakeCert = new FakeCertificate { SubjectName = "CN=Test" };
            var adapter = new CertificateAdapter(fakeCert);

            // Act
            var result = adapter.GetCertificate;

            // Assert
            result.Should().BeSameAs(fakeCert);
        }
    }
}
