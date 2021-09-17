using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;
using Toolbox.Tools.Zip;

namespace Spin.Common.Configuration
{
    /// <summary>
    /// Configuration store that uses drive paths to persists configuration data.
    ///
    /// "configStorePath" points to a folder location
    /// ".environment.json" is the required extension
    ///
    /// Each folder is an environment, environment names must be unique, e.g. dev-spin.environment.json.  The "dev-spin" is the environment
    /// and the "environment.json" is the required extension.
    ///
    /// </summary>
    public class ConfigurationStore
    {
        private readonly ILogger<ConfigurationStore> _logger;
        internal const string _extension = "-Spin.environment.json";
        internal const string _secretExtension = "-Spin.secret.json";


        public ConfigurationStore(ILogger<ConfigurationStore> logger)
        {
            _logger = logger.VerifyNotNull(nameof(logger));
        }

        public ConfigurationEnvironment Environment(string configStorePath, string environmentName) => new(configStorePath, environmentName, _logger);

        public ConfigurationBackup Backup(string configStorePath) => new(configStorePath, _logger);

        public Task<IReadOnlyList<string>> List(string configStorePath, CancellationToken token)
        {
            configStorePath.VerifyNotEmpty(nameof(configStorePath));

            ConfigurationStore.VerifyConfigStorePath(configStorePath);

            string[] files = Directory
                .GetFiles(configStorePath, "*" + ConfigurationStore._extension, SearchOption.TopDirectoryOnly)
                .Select(x => x.Replace(ConfigurationStore._extension, string.Empty))
                .ToArray();

            return Task.FromResult<IReadOnlyList<string>>(files);
        }

        static internal string GetConfigurationFile(string configStorePath, string environmentName)
        {
            VerifyConfigStorePath(configStorePath);
            VerifyEnvironmentName(environmentName);

            return Path.Combine(configStorePath, environmentName + _extension);
        }

        static internal string GetSecretFile(string configStorePath, string environmentName)
        {
            VerifyConfigStorePath(configStorePath);
            VerifyEnvironmentName(environmentName);

            return Path.Combine(configStorePath, environmentName + _secretExtension);
        }

        static internal void VerifyConfigStorePath(string configStorePath) => configStorePath
                .VerifyNotEmpty(nameof(configStorePath))
                .VerifyAssert(x => Directory.Exists(x), x => $"Folder {x} does not exist");

        static internal void VerifyEnvironmentName(string environmentName) =>
            environmentName
                .VerifyNotEmpty(nameof(environmentName))
                .All(x => char.IsLetterOrDigit(x) || x == '-')
                .VerifyAssert(x => true, $"{environmentName} is invalid, valid characters are alpha, numeric, and '-'");
    }
}