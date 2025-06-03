using CachingTools;
using CAPICOM;
using CryptographyTools.Signing;
using CryptographyTools.Signing.CryptoPro;
using CryptographyTools.Store;
using Ecp.Portal;
using Ecp.Web;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Decorators;
using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;
using FluentAssertions;
using Moq;
using WindowsTools;
using Xunit;

namespace EcpSigner.Infrastructure.Factories
{
    public class InfrastructureFactoryTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IConfigurationProviderFactory> _configFactoryMock = new();
        private readonly Mock<IWebClientFactory> _webClientFactoryMock = new();
        private readonly Mock<ICryptoFactory> _cryptoFactoryMock = new();
        private readonly Mock<IStoreFactory> _storeFactoryMock = new();
        private readonly Mock<ICacheFactory> _cacheFactoryMock = new();
        private readonly Mock<IFlashWindowFactory> _flashWindowFactoryMock = new();

        private readonly InfrastructureFactory _factory;

        public InfrastructureFactoryTests()
        {
            _factory = new InfrastructureFactory(
                _loggerMock.Object,
                _configFactoryMock.Object,
                _webClientFactoryMock.Object,
                _cryptoFactoryMock.Object,
                _storeFactoryMock.Object,
                _cacheFactoryMock.Object,
                _flashWindowFactoryMock.Object
            );
        }

        [Fact]
        public void CreateConfigurationProvider_ShouldReturnInstance()
        {
            // Arrange
            var configProviderMock = new Mock<IConfigurationProvider>();
            _configFactoryMock.Setup(x => x.Create()).Returns(configProviderMock.Object);

            // Act
            var result = _factory.CreateConfigurationProvider();

            // Assert
            result.Should().BeSameAs(configProviderMock.Object);
        }

        [Fact]
        public void CreatePortalService_ShouldReturnPortalServiceDecorator()
        {
            // Arrange
            var configMock = new Mock<IConfigurationProvider>();
            var configObject = new Domain.Models.AppSettings { url = "https://test.com", cacheMinutes = 10 };
            configMock.Setup(x => x.Get()).Returns(configObject);
            _configFactoryMock.Setup(x => x.Create()).Returns(configMock.Object);

            var webClientMock = new Mock<IClient>();
            _webClientFactoryMock.Setup(x => x.Create(configObject.url)).Returns(webClientMock.Object);

            // Act
            var result = _factory.CreatePortalService();

            // Assert
            result.Should().BeOfType<PortalServiceDecorator>();
        }

        [Fact]
        public void CreateSignatureService_ShouldReturnSignatureServiceDecorator()
        {
            // Arrange
            var cryptoMock = new Mock<ISigning>();
            var storeMock = new Mock<ICurrentUserStore>();

            _cryptoFactoryMock.Setup(x => x.Create()).Returns(cryptoMock.Object);
            _storeFactoryMock.Setup(x => x.Create()).Returns(storeMock.Object);

            // Act
            var result = _factory.CreateSignatureService();

            // Assert
            result.Should().BeOfType<SignatureServiceDecorator>();
        }

        [Fact]
        public void CreateCacheService_ShouldReturnCacheService()
        {
            // Arrange
            var configMock = new Mock<IConfigurationProvider>();
            var configObject = new Domain.Models.AppSettings { url = "url", cacheMinutes = 15 };
            configMock.Setup(x => x.Get()).Returns(configObject);
            _configFactoryMock.Setup(x => x.Create()).Returns(configMock.Object);

            var cacheMock = new Mock<ICache>();
            _cacheFactoryMock.Setup(x => x.Create(configObject.cacheMinutes)).Returns(cacheMock.Object);

            // Act
            var result = _factory.CreateCacheService();

            // Assert
            result.Should().BeOfType<CacheService>();
        }

        [Fact]
        public void CreateDatesService_ShouldReturnDatesService()
        {
            // Arrange
            var args = new[] { "arg1", "arg2" };

            // Act
            var result = _factory.CreateDatesService(args);

            // Assert
            result.Should().BeOfType<DatesService>();
        }

        [Fact]
        public void CreateFlashWindowService_ShouldReturnFlashWindowService()
        {
            // Arrange
            var flashMock = new Mock<IFlashWindow>();
            _flashWindowFactoryMock.Setup(x => x.Create()).Returns(flashMock.Object);

            // Act
            var result = _factory.CreateFlashWindowService();

            // Assert
            result.Should().BeOfType<FlashWindowService>();
        }

        [Fact]
        public void CreateDelayProvider_ShouldReturnDelayProvider()
        {
            // Act
            var result = _factory.CreateDelayProvider();

            // Assert
            result.Should().BeOfType<DelayProvider>();
        }

        [Fact]
        public void CreateDateTimeProvider_ShouldReturnDateTimeProvider()
        {
            // Act
            var result = _factory.CreateDateTimeProvider();

            // Assert
            result.Should().BeOfType<DateTimeProvider>();
        }
    }
}
