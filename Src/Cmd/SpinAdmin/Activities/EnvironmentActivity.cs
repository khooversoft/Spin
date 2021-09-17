using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;
using Toolbox.Extensions;

namespace SpinAdmin.Activities
{
    internal class EnvironmentActivity
    {
        private readonly ConfigurationStore _configurationStore;
        private readonly ILogger<EnvironmentActivity> _logger;

        public EnvironmentActivity(ConfigurationStore configurationStore, ILogger<EnvironmentActivity> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
        }

        public async Task List(string store, CancellationToken token)
        {
            IReadOnlyList<string> results = await _configurationStore.List(store, token);

            var list = new[]
            {
                $"{nameof(List)}: Listing environments",
                "",
            }
            .Concat(results.Select(y => $"Environment={y}"));

            _logger.LogInformation(list.Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine));
        }

        public async Task Delete(string store, string environment, CancellationToken token) => await _configurationStore
            .Environment(store, environment)
            .File
            .Delete(token);

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
