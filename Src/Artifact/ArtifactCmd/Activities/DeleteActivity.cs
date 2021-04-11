using ArtifactCmd.Application;
using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtifactCmd.Activities
{
    internal class DeleteActivity
    {
        private readonly Option _option;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<DeleteActivity> _logger;

        public DeleteActivity(Option option, IArtifactClient artifactClient, ILogger<DeleteActivity> logger)
        {
            _option = option;
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task Delete(CancellationToken token)
        {
            _logger.LogInformation($"Deleting {_option.Id} artifact...");

            bool removed = await _artifactClient.Delete((ArtifactId)_option.Id, token);

            _logger.LogInformation($"{(removed ? "Completed" : "Failed")} deleting {_option.Id}.");
        }
    }
}
