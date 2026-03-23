using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions
{
    public class ConfigurationException : BusinessException
    {
        public ConfigurationException(string configKey)
            : base(Statuses.ConfigurationError, errors: new { config_key = configKey })
        {
        }

        public ConfigurationException(string configKey, string detail)
            : base(Statuses.ConfigurationError, errors: new { config_key = configKey, details = detail })
        {
        }
    }
}
