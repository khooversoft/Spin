
using System;
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
    public class ConfigurationBackup
    {
        private readonly string _configStorePath;
        private readonly ILogger _logger;

        public ConfigurationBackup(string configStorePath, ILogger logger)
        {
            configStorePath.VerifyNotEmpty(nameof(configStorePath));
            logger.VerifyNotNull(nameof(logger));

            _configStorePath = configStorePath;
            _logger = logger;
        }

        public Task<string> Save(string? file, CancellationToken token)
        {
            ConfigurationStore.VerifyConfigStorePath(_configStorePath);

            string backupFile = file.ToNullIfEmpty() ?? Path.Combine(_configStorePath, ".backup", $"configStore.{Guid.NewGuid()}.bak.zip");
            Directory.CreateDirectory(Path.GetDirectoryName(backupFile).VerifyNotEmpty(nameof(backupFile)));

            CopyTo[] files = Directory
                .GetFiles(_configStorePath, "*" + ConfigurationStore._extension, SearchOption.TopDirectoryOnly)
                .Select(x => new CopyTo { Source = x, Destination = Path.GetFileName(x) })
                .ToArray();

            files.VerifyAssert(x => x.Length > 0, "No files to back up");

            new ZipFile(backupFile)
                .CompressFiles(token, files, x => _logger.LogTrace($"{nameof(Save)}: Compressing file {x}"));

            _logger.LogTrace($"{nameof(Save)}: environment configuration to {backupFile}");

            return Task.FromResult(backupFile);
        }

        public Task Restore(string backupFile, bool resetStore, CancellationToken token)
        {
            ConfigurationStore.VerifyConfigStorePath(_configStorePath);
            backupFile.VerifyNotEmpty(nameof(backupFile));

            if (resetStore)
            {
                _logger.LogTrace($"{nameof(Restore)}: reseting store at {_configStorePath}");
                Directory.Delete(_configStorePath, true);
            }

            new ZipFile(backupFile)
                .ExpandFiles(_configStorePath, token, x => _logger.LogTrace($"{nameof(Restore)}: Restoring file {x}"));

            _logger.LogTrace($"{nameof(Restore)}: environment configuration restored from {backupFile}");
            return Task.CompletedTask;
        }
    }
}
