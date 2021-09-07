using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinAdmin.Activities
{
    internal class StorageActivity
    {
        private readonly ConfigurationStore _configurationStore;
        private readonly ILogger<StorageActivity> _logger;

        public StorageActivity(ConfigurationStore configurationStore, ILogger<StorageActivity> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
        }

        public async Task Set(string store, string environment, string accountName, string containerName, CancellationToken token)
        {
            accountName.VerifyNotEmpty(nameof(accountName));
            containerName.VerifyNotEmpty(nameof(containerName));

            EnviromentConfigModel model = await _configurationStore.Get(store, environment, token) ?? new EnviromentConfigModel();
            model = model.AddWith(new StorageModel { AccountName = accountName, ContainerName = containerName });

            await _configurationStore.Set(store, environment, model, token);
        }

        public async Task Delete(string store, string environment, string accountName, string containerName, CancellationToken token)
        {
            accountName.VerifyNotEmpty(nameof(accountName));
            containerName.VerifyNotEmpty(nameof(containerName));

            EnviromentConfigModel model = await _configurationStore.Get(store, environment, token) ?? new EnviromentConfigModel();
            model = model.RemoveWith(new StorageModel { AccountName = accountName, ContainerName = containerName });

            await _configurationStore.Set(store, environment, model, token);
        }

        public async Task List(string store, string environment, CancellationToken token)
        {
            store.VerifyNotEmpty(nameof(store));
            environment.VerifyNotEmpty(nameof(environment));

            EnviromentConfigModel model = await _configurationStore.Get(store, environment, token) ?? new EnviromentConfigModel();

            var list = new[]
            {
                "Listing storage configurations",
                "",
            }
            .Concat((model.Storages ?? Array.Empty<StorageModel>()).Select(x => x.ToString()));

            _logger.LogInformation($"{nameof(List)}: {string.Join(Environment.NewLine, list)}");
        }
    }
}
