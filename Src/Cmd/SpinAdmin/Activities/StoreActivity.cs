using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;

namespace SpinAdmin.Activities
{
    internal class StoreActivity
    {
        private readonly ConfigurationStore _configurationStore;
        private readonly ILogger<EnvironmentActivity> _logger;

        public StoreActivity(ConfigurationStore configurationStore, ILogger<EnvironmentActivity> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
        }

        public async Task Backup(string store, string? file, CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Backup)}: Backing up configuration store");

            string backupFile = await _configurationStore
                .Backup(store)
                .Save(file, token);

            _logger.LogInformation($"{nameof(Backup)}: Configuration store has been backup to {backupFile}");
        }

        public async Task Restore(string store, string backupFile, bool resetStore, CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Restore)}: Restoring store");

            await _configurationStore
                .Backup(store)
                .Restore(backupFile, resetStore, token);

            _logger.LogInformation($"{nameof(Restore)}: Configuration store has been backup to {backupFile}");
        }
    }
}
