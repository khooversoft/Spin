using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;
using Spin.Common.Configuration.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinAdmin.Activities
{
    internal class QueueActivity
    {
        private readonly ConfigurationStore _configurationStore;
        private readonly ILogger<QueueActivity> _logger;

        public QueueActivity(ConfigurationStore configurationStore, ILogger<QueueActivity> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
        }

        public async Task Set(string store, string environment, QueueModel queueModel, CancellationToken token)
        {
            queueModel.Verify();

            EnvironmentModel model = await _configurationStore
                .Environment(store, environment)
                .File
                .Get(token) ?? new EnvironmentModel();

            model = model.AddWith(queueModel);

            await _configurationStore
                .Environment(store, environment)
                .File
                .Set(model, token);
        }

        public async Task Delete(string store, string environment, string channel, CancellationToken token)
        {
            channel.VerifyNotEmpty(nameof(channel));

            EnvironmentModel model = await _configurationStore
                .Environment(store, environment)
                .File
                .Get(token) ?? new EnvironmentModel();

            model = model.RemoveWith(channel);

            await _configurationStore
                .Environment(store, environment)
                .File
                .Set(model, token);
        }

        public async Task List(string store, string environment, CancellationToken token)
        {
            store.VerifyNotEmpty(nameof(store));
            environment.VerifyNotEmpty(nameof(environment));

            EnvironmentModel model = await _configurationStore
                .Environment(store, environment)
                .File
                .Get(token) ?? new EnvironmentModel();

            var list = new[]
            {
                "Listing queue configurations",
                "",
            }
            .Concat((model.Queue ?? Array.Empty<QueueModel>()).Select(x => x.ToString()));

            _logger.LogInformation($"{nameof(List)}: {string.Join(Environment.NewLine, list)}");
        }
    }
}
