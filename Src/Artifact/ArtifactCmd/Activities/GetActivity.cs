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
using Toolbox.Tools;

namespace ArtifactCmd.Activities
{
    internal class GetActivity
    {
        private readonly Option _option;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<GetActivity> _logger;

        public GetActivity(Option option, IArtifactClient artifactClient, ILogger<GetActivity> logger)
        {
            _option = option;
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task Set(CancellationToken token)
        {
            _logger.LogInformation($"Reading {_option.File} from artifact id={_option.Id}...");

            ArtifactId artifactId = new ArtifactId(_option.Id!);
            ArtifactPayload? payload = await _artifactClient.Get(artifactId, token);
            if( payload == null)
            {
                _logger.LogError($"Artifact {artifactId} does not exist");
                return;
            }

            byte[] bytes = payload.ToBytes();
            File.WriteAllBytes(_option.File, bytes);

            _logger.LogInformation($"Completed download {_option.File} from artifact id={_option.Id}.");
        }
    }
}
