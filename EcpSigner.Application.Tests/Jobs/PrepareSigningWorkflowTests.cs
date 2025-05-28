using EcpSigner.Application.Filters;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Jobs
{
    public class PrepareSigningWorkflowTests
    {
        private readonly Mock<IPortalService> _portalServiceMock = new();
        private readonly Mock<ISignatureService> _signatureServiceMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IConfigurationProvider> _configProviderMock = new();
        private readonly Mock<IDatesService> _datesServiceMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<IFlashWindowService> _flashWindowServiceMock = new();
        private readonly Mock<ISignDocumentsLoop> _signDocumentsLoopMock = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

        private PrepareSigningWorkflow CreateSut()
        {
            return new PrepareSigningWorkflow(
                _portalServiceMock.Object,
                _signatureServiceMock.Object,
                _loggerMock.Object,
                _configProviderMock.Object,
                _datesServiceMock.Object,
                _cacheServiceMock.Object,
                _flashWindowServiceMock.Object,
                _signDocumentsLoopMock.Object,
                _dateTimeProviderMock.Object);
        }

        [Fact]
        public async Task RunAsync_ShouldLoginAndSignDocuments_WhenNotLoggedIn()
        {
            // Arrange
            var sut = CreateSut();

            var config = new AppSettings { login = "user", password = "pass", ignoreDocTypesDict = new Dictionary<string, byte>() };
            var docs = new List<Document> {
                new() { ID = "1", Type = "Type1", SignStatus = "2", Error = null },
            };

            _configProviderMock.Setup(c => c.Get()).Returns(config);
            _datesServiceMock.Setup(d => d.GetDates()).Returns(("2024-01-01", "2024-01-31"));
            _portalServiceMock.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _portalServiceMock.Setup(p => p.SearchDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(docs);
            _cacheServiceMock.Setup(c => c.Contains("1")).Returns(false);

            _signatureServiceMock.Setup(s => s.GetUserCertificates()).Returns(new Dictionary<string, ICertificate>
            {
                { "thumb1", Mock.Of<ICertificate>() }
            });

            _portalServiceMock.Setup(p => p.LoadEcpCertificates()).ReturnsAsync(new List<EcpCertificate>
            {
                new() { thumbprint = "thumb1" }
            });

            _signatureServiceMock.Setup(s => s.Sign(It.IsAny<ICertificate>(), It.IsAny<string>(), It.IsAny<string>()));
            _signDocumentsLoopMock.Setup(l => l.RunAsync(It.IsAny<List<Document>>(), It.IsAny<List<(EcpCertificate, ICertificate)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((1, new List<string>()));

            _dateTimeProviderMock.SetupSequence(d => d.Now).Returns(new Queue<DateTime>(new[] { DateTime.Now, DateTime.Now - TimeSpan.FromDays(1) }).Dequeue);

            // Act
            Func<Task> act = async () => await sut.RunAsync(CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _cacheServiceMock.Verify(c => c.RemoveExpired(), Times.Once);
            _portalServiceMock.Verify(p => p.Login("user", "pass"), Times.Once);
            _portalServiceMock.Verify(p => p.SearchDocuments("2024-01-01", "2024-01-31", CancellationToken.None), Times.Once);
            _flashWindowServiceMock.Verify(f => f.Stop(), Times.Once);
            _loggerMock.Verify(l => l.Flush(), Times.Once);
        }

        [Fact]
        public async Task RunAsync_ShouldLoginAndSignDocuments_WhenNotLoggedIn_WithDifferentDocuments()
        {
            // Arrange
            var sut = CreateSut();

            var config = new AppSettings { login = "user", password = "pass", ignoreDocTypesDict = new Dictionary<string, byte>() { { "Type1", 1 } } };
            var docs = new List<Document> {
                new() { ID = "1", Type = "Type1", SignStatus = "2", Error = null },
                new() { ID = "2", Type = "Type2", SignStatus = "2", Error = null },
                new() { ID = "2", Type = "Type2", SignStatus = "2", Error = "test" },
            };

            _configProviderMock.Setup(c => c.Get()).Returns(config);
            _datesServiceMock.Setup(d => d.GetDates()).Returns(("2024-01-01", "2024-01-31"));
            _portalServiceMock.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _portalServiceMock.Setup(p => p.SearchDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(docs);
            _cacheServiceMock.Setup(c => c.Contains("1")).Returns(false);

            _signatureServiceMock.Setup(s => s.GetUserCertificates()).Returns(new Dictionary<string, ICertificate>
            {
                { "thumb1", Mock.Of<ICertificate>() }
            });

            _portalServiceMock.Setup(p => p.LoadEcpCertificates()).ReturnsAsync(new List<EcpCertificate>
            {
                new() { thumbprint = "thumb1" }
            });

            _signatureServiceMock.Setup(s => s.Sign(It.IsAny<ICertificate>(), It.IsAny<string>(), It.IsAny<string>()));
            _signDocumentsLoopMock.Setup(l => l.RunAsync(It.IsAny<List<Document>>(), It.IsAny<List<(EcpCertificate, ICertificate)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((1, new List<string>()));

            // Act
            Func<Task> act = async () => await sut.RunAsync(CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _portalServiceMock.Verify(p => p.Login("user", "pass"), Times.Once);
            _portalServiceMock.Verify(p => p.SearchDocuments("2024-01-01", "2024-01-31", CancellationToken.None), Times.Once);
            _flashWindowServiceMock.Verify(f => f.Start(), Times.Once);
            _loggerMock.Verify(l => l.Flush(), Times.Once);
        }

        [Fact]
        public async Task RunAsync_ShouldLoginAndThrowContinueException_WhenSigningNoDocuments()
        {
            // Arrange
            var sut = CreateSut();

            var config = new AppSettings { login = "user", password = "pass", ignoreDocTypesDict = new Dictionary<string, byte>() };
            var docs = new List<Document>();

            _configProviderMock.Setup(c => c.Get()).Returns(config);
            _datesServiceMock.Setup(d => d.GetDates()).Returns(("2024-01-01", "2024-01-31"));
            _portalServiceMock.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _portalServiceMock.Setup(p => p.SearchDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(docs);
            _cacheServiceMock.Setup(c => c.Contains("1")).Returns(false);

            _signatureServiceMock.Setup(s => s.GetUserCertificates()).Returns(new Dictionary<string, ICertificate>
            {
                { "thumb1", Mock.Of<ICertificate>() }
            });

            _portalServiceMock.Setup(p => p.LoadEcpCertificates()).ReturnsAsync(new List<EcpCertificate>
            {
                new() { thumbprint = "thumb1" }
            });

            _signatureServiceMock.Setup(s => s.Sign(It.IsAny<ICertificate>(), It.IsAny<string>(), It.IsAny<string>()));
            _signDocumentsLoopMock.Setup(l => l.RunAsync(It.IsAny<List<Document>>(), It.IsAny<List<(EcpCertificate, ICertificate)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((1, new List<string>()));

            _dateTimeProviderMock.SetupSequence(d => d.Now).Returns(new Queue<DateTime>(new[] { DateTime.Now, DateTime.Now - TimeSpan.FromDays(1) }).Dequeue);

            // Act
            Func<Task> act = async () => await sut.RunAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ContinueException>()
                .WithMessage("не найдены документы для отправки");
        }
        [Fact]
        public async Task RunAsync_ShouldThrow_IsNotLoggedInExceptionThrown()
        {
            // Arrange
            var sut = CreateSut();

            var config = new AppSettings { login = "user", password = "pass", ignoreDocTypesDict = new Dictionary<string, byte>() };
            var docs = new List<Document> { new() { ID = "1", Type = "Type1", SignStatus = "2", Error = null } };

            _configProviderMock.Setup(c => c.Get()).Returns(config);
            _datesServiceMock.Setup(d => d.GetDates()).Returns(("2024-01-01", "2024-01-31"));
            _portalServiceMock.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Throws(new IsNotLoggedInException("пароль неверный"));

            // Act
            Func<Task> act = async () => await sut.RunAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<IsNotLoggedInException>()
                .WithMessage("пароль неверный");
        }

        [Fact]
        public async Task GetCertificates_ShouldThrow_WhenNoMatchedCertificates()
        {
            // Arrange
            var sut = CreateSut();

            var userCerts = new Dictionary<string, ICertificate>();
            var ecpCerts = new List<EcpCertificate> { new() { thumbprint = "thumb1" } };

            var privateMethod = typeof(PrepareSigningWorkflow)
                .GetMethod("GetMatchedCertificates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Action act = () => privateMethod.Invoke(sut, new object[] { ecpCerts, userCerts });

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<BreakWorkException>()
                .WithMessage("*не найдены*");
        }

        [Fact]
        public void GetSuitableCertificates_ShouldThrow_WhenNoSuitable()
        {
            // Arrange
            var sut = CreateSut();

            var certPair = (new EcpCertificate { thumbprint = "abc" }, Mock.Of<ICertificate>());
            _signatureServiceMock.Setup(x => x.Sign(It.IsAny<ICertificate>(), "test", "test")).Throws<Exception>();

            var method = typeof(PrepareSigningWorkflow)
                .GetMethod("GetSuitableCertificates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Action act = () => method.Invoke(sut, new object[] { new List<(EcpCertificate, ICertificate)> { certPair } });

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<BreakWorkException>()
                .WithMessage("*не найдены*");
        }

        [Fact]
        public async Task RunAsync_ShouldThrow_WhenLoadNoCertificates()
        {
            // Arrange
            var sut = CreateSut();

            var config = new AppSettings { login = "user", password = "pass", ignoreDocTypesDict = new Dictionary<string, byte>() };
            var docs = new List<Document> {
                new() { ID = "1", Type = "Type1", SignStatus = "2", Error = null },
            };

            _configProviderMock.Setup(c => c.Get()).Returns(config);
            _datesServiceMock.Setup(d => d.GetDates()).Returns(("2024-01-01", "2024-01-31"));
            _portalServiceMock.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _portalServiceMock.Setup(p => p.SearchDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(docs);
            _cacheServiceMock.Setup(c => c.Contains("1")).Returns(false);

            _signatureServiceMock.Setup(s => s.GetUserCertificates()).Returns(new Dictionary<string, ICertificate>
            {
                { "thumb1", Mock.Of<ICertificate>() }
            });

            _portalServiceMock.Setup(p => p.LoadEcpCertificates()).ReturnsAsync(new List<EcpCertificate> ());

            _signatureServiceMock.Setup(s => s.Sign(It.IsAny<ICertificate>(), It.IsAny<string>(), It.IsAny<string>()));
            _signDocumentsLoopMock.Setup(l => l.RunAsync(It.IsAny<List<Document>>(), It.IsAny<List<(EcpCertificate, ICertificate)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((1, new List<string>()));

            _dateTimeProviderMock.SetupSequence(d => d.Now).Returns(new Queue<DateTime>(new[] { DateTime.Now, DateTime.Now - TimeSpan.FromDays(1) }).Dequeue);

            // Act
            Func<Task> act = async () => await sut.RunAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BreakWorkException>()
                .WithMessage("*не обнаружены*");
        }

        [Fact]
        public void TryLogin_ShouldThrow_WhenTaskCanceled()
        {
            // Arrange
            var sut = CreateSut();

            var method = typeof(PrepareSigningWorkflow)
                .GetMethod("TryLogin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var source = new CancellationTokenSource();
            source.Cancel();

            // Act
            Func<Task> act = async () => await (Task)method.Invoke(sut, [source.Token]);

            // Assert
            act.Should().ThrowAsync<StopWorkException>();
        }

        [Fact]
        public void GetDocuments_ShouldThrow_WhenTaskCanceled()
        {
            // Arrange
            var sut = CreateSut();

            _portalServiceMock.Setup(p => p.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var method = typeof(PrepareSigningWorkflow)
                .GetMethod("GetDocuments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var source = new CancellationTokenSource();
            source.Cancel();

            // Act
            Func<Task> act = async () => await (Task)method.Invoke(sut, [It.IsAny<string>(), It.IsAny<string>(), source.Token]);

            // Assert
            act.Should().ThrowAsync<StopWorkException>();
        }
    }
}
