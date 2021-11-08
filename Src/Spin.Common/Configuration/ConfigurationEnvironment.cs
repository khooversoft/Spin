//using Microsoft.Extensions.Logging;

//namespace Spin.Common.Configuration
//{
//    public class ConfigurationEnvironment
//    {
//        public ConfigurationEnvironment(string configStorePath, string environmentName, ILogger logger)
//        {
//            ConfigStorePath = configStorePath;
//            EnvironmentName = environmentName;

//            File = new ConfigurationFile(configStorePath, environmentName, logger);
//            Secret = new ConfigurationSecret(configStorePath, environmentName, logger);
//        }

//        public string ConfigStorePath { get; }

//        public string EnvironmentName { get; }

//        public ConfigurationFile File { get; private set; }

//        public ConfigurationSecret Secret { get; private set; }
//    }
//}