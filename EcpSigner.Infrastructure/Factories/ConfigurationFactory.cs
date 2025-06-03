using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public class ConfigurationProviderFactory : IConfigurationProviderFactory
    {
        private readonly ILogger _logger;
        private readonly string _configPath;
        public ConfigurationProviderFactory(ILogger logger, string configPath) 
        {
            _logger = logger;
            _configPath = configPath;
        }
        public IConfigurationProvider Create()
        {
            return new JsonConfigurationProvider(_logger, _configPath);
        }
    }
}
