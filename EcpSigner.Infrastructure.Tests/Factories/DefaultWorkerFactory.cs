using Xunit;
using Moq;
using FluentAssertions;
using EcpSigner.Application.Interfaces;
using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Workers;
using EcpSigner.Infrastructure.Services;
using EcpSigner.Application.Jobs;
using EcpSigner.Domain.Interfaces;

namespace EcpSigner.Infrastructure.Factories
{
    public class DefaultWorkerFactoryTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IInfrastructureFactory> _infraFactoryMock = new();

        private readonly Mock<IConfigurationProvider> _configMock = new();
        private readonly Mock<IPortalService> _portalServiceMock = new();
        private readonly Mock<ISignatureService> _signatureServiceMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<IDatesService> _datesServiceMock = new();
        private readonly Mock<IFlashWindowService> _flashWindowServiceMock = new();
        private readonly Mock<IDelayProvider> _delayProviderMock = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

        private readonly string[] _args = new[] { "arg1", "arg2" };

        public DefaultWorkerFactoryTests()
        {
            _infraFactoryMock.Setup(x => x.CreateConfigurationProvider()).Returns(_configMock.Object);
            _infraFactoryMock.Setup(x => x.CreatePortalService()).Returns(_portalServiceMock.Object);
            _infraFactoryMock.Setup(x => x.CreateSignatureService()).Returns(_signatureServiceMock.Object);
            _infraFactoryMock.Setup(x => x.CreateCacheService()).Returns(_cacheServiceMock.Object);
            _infraFactoryMock.Setup(x => x.CreateDatesService(It.IsAny<string[]>())).Returns(_datesServiceMock.Object);
            _infraFactoryMock.Setup(x => x.CreateFlashWindowService()).Returns(_flashWindowServiceMock.Object);
            _infraFactoryMock.Setup(x => x.CreateDelayProvider()).Returns(_delayProviderMock.Object);
            _infraFactoryMock.Setup(x => x.CreateDateTimeProvider()).Returns(_dateTimeProviderMock.Object);
        }

        [Fact]
        public void CreateWorker_ShouldReturnDocumentSigningWorker()
        {
            // Arrange
            var factory = new DefaultWorkerFactory(_loggerMock.Object, _infraFactoryMock.Object);

            // Act
            var result = factory.CreateWorker(_args);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<DocumentSigningWorker>();
        }

        [Fact]
        public void CreateWorker_ShouldCallAllFactoryMethods()
        {
            // Arrange
            var factory = new DefaultWorkerFactory(_loggerMock.Object, _infraFactoryMock.Object);

            // Act
            factory.CreateWorker(_args);

            // Assert
            _infraFactoryMock.Verify(x => x.CreateConfigurationProvider(), Times.Once);
            _infraFactoryMock.Verify(x => x.CreatePortalService(), Times.Once);
            _infraFactoryMock.Verify(x => x.CreateSignatureService(), Times.Once);
            _infraFactoryMock.Verify(x => x.CreateCacheService(), Times.Once);
            _infraFactoryMock.Verify(x => x.CreateDatesService(_args), Times.Once);
            _infraFactoryMock.Verify(x => x.CreateFlashWindowService(), Times.Once);
            _infraFactoryMock.Verify(x => x.CreateDelayProvider(), Times.AtLeastOnce);
            _infraFactoryMock.Verify(x => x.CreateDateTimeProvider(), Times.Once);
        }
    }
}
