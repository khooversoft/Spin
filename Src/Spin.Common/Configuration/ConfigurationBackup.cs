
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
        private readonly ILogger _logger;

        public ConfigurationBackup(ILogger logger)
        {
            _logger = logger;
        }

        public Task<string> Save(string configStorePath, string? file, CancellationToken token)
        {
            ConfigurationStore.VerifyConfigStorePath(configStorePath);

            string backupFile = file.ToNullIfEmpty() ?? Path.Combine(configStorePath, ".backup", $"configStore.{Guid.NewGuid()}.bak.zip");
            Directory.CreateDirectory(Path.GetDirectoryName(backupFile).VerifyNotEmpty(nameof(backupFile)));

            CopyTo[] files = Directory
                .GetFiles(configStorePath, "*" + ConfigurationStore._extension, SearchOption.TopDirectoryOnly)
                .Select(x => new CopyTo { Source = x, Destination = Path.GetFileName(x) })
                .ToArray();

            files.VerifyAssert(x => x.Length > 0, "No files to back up");

            new ZipFile(backupFile)
                .CompressFiles(token, files, x => _logger.LogTrace($"{nameof(Save)}: Compressing file {x}"));

            _logger.LogTrace($"{nameof(Save)}: environment configuration to {backupFile}");

            return Task.FromResult(backupFile);
        }

        public Task Restore(string configStorePath, string backupFile, bool resetStore, CancellationToken token)
        {
            ConfigurationStore.VerifyConfigStorePath(configStorePath);
            backupFile.VerifyNotEmpty(nameof(backupFile));

            if (resetStore)
            {
                _logger.LogTrace($"{nameof(Restore)}: reseting store at {configStorePath}");
                Directory.Delete(configStorePath, true);
            }

            new ZipFile(backupFile)
                .ExpandFiles(configStorePath, token, x => _logger.LogTrace($"{nameof(Restore)}: Restoring file {x}"));

            _logger.LogTrace($"{nameof(Restore)}: environment configuration restored from {backupFile}");
            return Task.CompletedTask;
        }
    }
}
