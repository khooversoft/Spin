using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Spin.Common.Configuration
{
    public class ConfigurationFile
    {
        private readonly string _configStorePath;
        private readonly string _environmentName;
        private readonly ILogger _logger;

        internal ConfigurationFile(string configStorePath, string environmentName, ILogger logger)
        {
            configStorePath.VerifyNotEmpty(nameof(configStorePath));
            environmentName.VerifyNotEmpty(nameof(environmentName));
            logger.VerifyNotNull(nameof(logger));

            _configStorePath = configStorePath;
            _environmentName = environmentName;
            _logger = logger;
        }

        public async Task<EnviromentConfigModel?> Get(CancellationToken token)
        {
            string fullPath = ConfigurationStore.GetConfigurationFile(_configStorePath, _environmentName);

            if (!File.Exists(fullPath))
            {
                _logger.LogTrace($"{nameof(Get)}: environment configuration for {_environmentName} from {_configStorePath} does not exist");
                return null;
            }

            string json = await File.ReadAllTextAsync(fullPath, token);
            _logger.LogTrace($"{nameof(Get)}: environment configuration for {_environmentName} from {_configStorePath}");

            return Json.Default.Deserialize<EnviromentConfigModel>(json);
        }

        public async Task Set(EnviromentConfigModel model, CancellationToken token)
        {
            model.Verify();
            string fullPath = ConfigurationStore.GetConfigurationFile(_configStorePath, _environmentName);

            string json = Json.Default.SerializeFormat(model);
            await File.WriteAllTextAsync(fullPath, json, token);

            _logger.LogTrace($"{nameof(Set)}: environment configuration for {_environmentName} to {_configStorePath}");
        }

        public Task Delete(CancellationToken token)
        {
            string fullPath = ConfigurationStore.GetConfigurationFile(_configStorePath, _environmentName);

            if (File.Exists(fullPath)) File.Delete(fullPath);
            _logger.LogTrace($"{nameof(Delete)}: environment configuration for {_environmentName} to {_configStorePath}");

            return Task.CompletedTask;
        }
    }
}
