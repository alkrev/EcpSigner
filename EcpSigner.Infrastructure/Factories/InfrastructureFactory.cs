using CachingTools;
using CryptographyTools.Signing.CryptoPro;
using CryptographyTools.Store;
using Ecp.Portal;
using Ecp.Web;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Configuration;
using EcpSigner.Infrastructure.Decorators;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Infrastructure.Services;
using EcpSigner.Infrastructure.WebClients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsTools;

namespace EcpSigner.Infrastructure.Factories
{
    public class InfrastructureFactory : IInfrastructureFactory
    {
        private readonly string _configPath;
        private readonly ILogger _logger;

        public InfrastructureFactory(ILogger logger, string configPath) 
        {
            _configPath = configPath;
            _logger = logger;
        }

        public IConfigurationProvider CreateConfigurationProvider()
        {
            return new JsonConfigurationProvider(_logger, _configPath);
        }
        public IPortalService CreatePortalService()
        {
            var config = CreateConfigurationProvider();
            var webClient = new WebClient(new Client(config.Get().url));
            var portalService = new PortalService(new Main(webClient), new EMD(webClient));
            return new PortalServiceDecorator(portalService, _logger);
        }
        public ISignatureService CreateSignatureService()
        {
            var crypto = new Crypto();
            var store = new CurrentUserStore();
            var signatureService = new SignatureService(crypto, store);
            return new SignatureServiceDecorator(signatureService, _logger);
        }
        public ICacheService CreateCacheService()
        {
            var config = CreateConfigurationProvider();
            var cache = new Cache(config.Get().cacheMinutes);
            return new CacheService(cache);
        }
        public IDatesService CreateDatesService(string[] args)
        {
            return new DatesService(args);
        }
        public IFlashWindowService CreateFlashWindowService()
        {
            var flashWindow = new FlashWindow(Process.GetCurrentProcess().MainWindowHandle);
            return new FlashWindowService(flashWindow);
        }
        public IDelayProvider CreateDelayProvider()
        {
            return new DelayProvider();
        }
        public IDateTimeProvider CreateDateTimeProvider()
        {
            return new DateTimeProvider();
        }
    }
}
