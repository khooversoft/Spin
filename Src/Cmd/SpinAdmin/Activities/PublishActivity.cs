using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;

namespace SpinAdmin.Activities
{
    internal class PublishActivity
    {
        private readonly ConfigurationStore _configurationStore;
        private readonly ILogger<PublishActivity> _logger;

        public PublishActivity(ConfigurationStore configurationStore, ILogger<PublishActivity> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
        }

        public async Task Publish(string store, string environment, CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Publish)}: Publishing configuration");

            EnviromentConfigModel? model = await _configurationStore.Environment.Get(store, environment, token);
            if( model == null)
            {
                _logger.LogInformation($"{nameof(Publish)}: No configuration file found");
                return;
            }

            await StorageActivity(model);
        }

        private async Task StorageActivity(EnviromentConfigModel model)
        {
            throw new NotImplementedException();
        }
    }
}
