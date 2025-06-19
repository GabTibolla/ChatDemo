using Microsoft.Extensions.Configuration;

namespace ChatDemo.Business.Services
{
    public class ConfigService : ChatDemo.Business.Interfaces.IConfigService
    {
        private readonly IConfiguration _configuration;

        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T? Get<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }
    }
}
