using Ecp.Portal;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Decorators;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Infrastructure.Services;

namespace EcpSigner.Infrastructure.Factories
{
    public class InfrastructureFactory : IInfrastructureFactory
    {
        private readonly ILogger _logger;
        private readonly IConfigurationProviderFactory _configurationFactory;
        private readonly IWebClientFactory _webClientFactory;
        private readonly ICryptoFactory _cryptoFactory;
        private readonly IStoreFactory _storeFactory;
        private readonly ICacheFactory _cacheFactory;
        private readonly IFlashWindowFactory _flashWindowFactory;
        private readonly IDateTimeProviderFactory _dateTimeProviderFactory;

        public InfrastructureFactory(
            ILogger logger,
            IConfigurationProviderFactory configurationFactory,
            IWebClientFactory webClientFactory,
            ICryptoFactory cryptoFactory,
            IStoreFactory storeFactory,
            ICacheFactory cacheFactory,
            IFlashWindowFactory flashWindowFactory,
            IDateTimeProviderFactory dateTimeProviderFactory
        ) 
        {
            _logger = logger;
            _configurationFactory = configurationFactory;
            _webClientFactory = webClientFactory;
            _cryptoFactory = cryptoFactory;
            _storeFactory = storeFactory;
            _cacheFactory = cacheFactory;
            _flashWindowFactory = flashWindowFactory;
            _dateTimeProviderFactory = dateTimeProviderFactory;
        }

        public IConfigurationProvider CreateConfigurationProvider()
        {
            return _configurationFactory.Create();
        }
        public IPortalService CreatePortalService()
        {
            var config = CreateConfigurationProvider();
            var webClient = _webClientFactory.Create(config.Get().url);
            var portalService = new PortalService(new Main(webClient), new EMD(webClient));
            return new PortalServiceDecorator(portalService, _logger);
        }
        public ISignatureService CreateSignatureService()
        {
            var crypto = _cryptoFactory.Create();
            var store = _storeFactory.Create();
            var signatureService = new SignatureService(crypto, store);
            return new SignatureServiceDecorator(signatureService, _logger);
        }
        public ICacheService CreateCacheService()
        {
            var config = CreateConfigurationProvider();
            var dateTimeProvider = _dateTimeProviderFactory.Create();
            var cache = _cacheFactory.Create(config.Get().cacheMinutes, dateTimeProvider);
            return new CacheService(cache);
        }
        public IDatesService CreateDatesService(string[] args)
        {
            var dateTimeProvider = _dateTimeProviderFactory.Create();
            return new DatesService(args, dateTimeProvider);
        }
        public IFlashWindowService CreateFlashWindowService()
        {
            var flashWindow = _flashWindowFactory.Create();
            return new FlashWindowService(flashWindow);
        }
        public IDelayProvider CreateDelayProvider()
        {
            return new DelayProvider();
        }
        public IDateTimeProvider CreateDateTimeProvider()
        {
            return _dateTimeProviderFactory.Create();
        }
    }
}
