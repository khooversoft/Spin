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
        internal const string _extension = ".environment.json";
        internal const string _secretExtension = ".secret.json";


        public ConfigurationStore(ILogger<ConfigurationStore> logger)
        {
            _logger = logger.VerifyNotNull(nameof(logger));

            Environment = new ConfigurationFile(logger);
            Backup = new ConfigurationBackup(logger);
            Secret = new ConfigurationSecret(logger);
        }

        public ConfigurationFile Environment { get; private set; } 

        public ConfigurationBackup Backup { get; private set; }

        public ConfigurationSecret Secret { get; private set; }


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