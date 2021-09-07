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
        private const string _extension = ".environment.json";

        public ConfigurationStore(ILogger<ConfigurationStore> logger)
        {
            _logger = logger.VerifyNotNull(nameof(logger));
        }

        public async Task<EnviromentConfigModel?> Get(string configStorePath, string environmentName, CancellationToken token)
        {
            string fullPath = GetConfigurationFile(configStorePath, environmentName);

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
            string fullPath = GetConfigurationFile(configStorePath, environmentName);

            string json = Json.Default.SerializeFormat(model);
            await File.WriteAllTextAsync(fullPath, json, token);

            _logger.LogTrace($"{nameof(Set)}: environment configuration for {environmentName} to {configStorePath}");
        }

        public Task DeleteEnvironment(string configStorePath, string environmentName, CancellationToken token)
        {
            string fullPath = GetConfigurationFile(configStorePath, environmentName);

            if (File.Exists(fullPath)) File.Delete(fullPath);
            _logger.LogTrace($"{nameof(DeleteEnvironment)}: environment configuration for {environmentName} to {configStorePath}");

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> ListEnvironments(string configStorePath, CancellationToken token)
        {
            VerifyConfigStorePath(configStorePath);

            string[] files = Directory
                .GetFiles(configStorePath, "*" + _extension, SearchOption.TopDirectoryOnly)
                .Select(x => x.Replace(_extension, string.Empty))
                .ToArray();

            return Task.FromResult<IReadOnlyList<string>>(files);
        }

        public Task<string> Backup(string configStorePath, string? file, CancellationToken token)
        {
            VerifyConfigStorePath(configStorePath);

            string backupFile = file.ToNullIfEmpty() ?? Path.Combine(configStorePath, ".backup", $"configStore.{Guid.NewGuid()}.bak.zip");
            Directory.CreateDirectory(Path.GetDirectoryName(backupFile).VerifyNotEmpty(nameof(backupFile)));

            CopyTo[] files = Directory
                .GetFiles(configStorePath, "*" + _extension, SearchOption.TopDirectoryOnly)
                .Select(x => new CopyTo { Source = x, Destination = Path.GetFileName(x) })
                .ToArray();

            files.VerifyAssert(x => x.Length > 0, "No files to back up");

            new ZipFile(backupFile)
                .CompressFiles(token, files, x => _logger.LogTrace($"{nameof(Backup)}: Compressing file {x}"));

            _logger.LogTrace($"{nameof(Backup)}: environment configuration to {backupFile}");

            return Task.FromResult(backupFile);
        }

        public Task RestoreBackup(string configStorePath, string backupFile, bool resetStore, CancellationToken token)
        {
            VerifyConfigStorePath(configStorePath);
            backupFile.VerifyNotEmpty(nameof(backupFile));

            if (resetStore)
            {
                _logger.LogTrace($"{nameof(RestoreBackup)}: reseting store at {configStorePath}");
                Directory.Delete(configStorePath, true);
            }

            new ZipFile(backupFile)
                .ExpandFiles(configStorePath, token, x => _logger.LogTrace($"{nameof(RestoreBackup)}: Restoring file {x}"));

            _logger.LogTrace($"{nameof(RestoreBackup)}: environment configuration restored from {backupFile}");
            return Task.CompletedTask;
        }

        private string GetConfigurationFile(string configStorePath, string environmentName)
        {
            VerifyConfigStorePath(configStorePath);
            VerifyEnvironmentName(environmentName);

            return Path.Combine(configStorePath, environmentName + _extension);
        }

        private void VerifyConfigStorePath(string configStorePath) => configStorePath
                .VerifyNotEmpty(nameof(configStorePath))
                .VerifyAssert(x => Directory.Exists(x), x => $"Folder {x} does not exist");

        private void VerifyEnvironmentName(string environmentName) =>
            environmentName
                .VerifyNotEmpty(nameof(environmentName))
                .All(x => char.IsLetterOrDigit(x) || x == '-')
                .VerifyAssert(x => true, $"{environmentName} is invalid, valid characters are alpha, numeric, and '-'");
    }
}