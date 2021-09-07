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
        private readonly ILogger _logger;

        internal ConfigurationFile(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<EnviromentConfigModel?> Get(string configStorePath, string environmentName, CancellationToken token)
        {
            string fullPath = ConfigurationStore.GetConfigurationFile(configStorePath, environmentName);

            if (!File.Exists(fullPath))
            {
                _logger.LogTrace($"{nameof(Get)}: environment configuration for {environmentName} from {configStorePath} does not exist");
                return null;
            }

            string json = await File.ReadAllTextAsync(fullPath, token);
            _logger.LogTrace($"{nameof(Get)}: environment configuration for {environmentName} from {configStorePath}");

            return Json.Default.Deserialize<EnviromentConfigModel>(json);
        }

        public async Task Set(string configStorePath, string environmentName, EnviromentConfigModel model, CancellationToken token)
        {
            model.Verify();
            string fullPath = ConfigurationStore.GetConfigurationFile(configStorePath, environmentName);

            string json = Json.Default.SerializeFormat(model);
            await File.WriteAllTextAsync(fullPath, json, token);

            _logger.LogTrace($"{nameof(Set)}: environment configuration for {environmentName} to {configStorePath}");
        }

        public Task Delete(string configStorePath, string environmentName, CancellationToken token)
        {
            string fullPath = ConfigurationStore.GetConfigurationFile(configStorePath, environmentName);

            if (File.Exists(fullPath)) File.Delete(fullPath);
            _logger.LogTrace($"{nameof(Delete)}: environment configuration for {environmentName} to {configStorePath}");

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> List(string configStorePath, CancellationToken token)
        {
            ConfigurationStore.VerifyConfigStorePath(configStorePath);

            string[] files = Directory
                .GetFiles(configStorePath, "*" + ConfigurationStore._extension, SearchOption.TopDirectoryOnly)
                .Select(x => x.Replace(ConfigurationStore._extension, string.Empty))
                .ToArray();

            return Task.FromResult<IReadOnlyList<string>>(files);
        }
    }
}
