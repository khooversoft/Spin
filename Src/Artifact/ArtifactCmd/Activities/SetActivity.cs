using ArtifactCmd.Application;
using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtifactCmd.Activities
{
    internal class SetActivity
    {
        private readonly Option _option;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<SetActivity> _logger;

        public SetActivity(Option option, IArtifactClient artifactClient, ILogger<SetActivity> logger)
        {
            _option = option;
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task Set(CancellationToken token)
        {
            _logger.LogInformation($"Writing {_option.File} to artifact id={_option.Id}...");

            ArtifactId artifactId = new ArtifactId(_option.Id!);
            byte[] bytes = await File.ReadAllBytesAsync(_option.File, token);

            ArtifactPayload payload = bytes.ToArtifactPayload(artifactId);
            await _artifactClient.Set(payload, token);

            _logger.LogInformation($"Completed writing {_option.File} to artifact id={_option.Id}.");
        }
    }
}
