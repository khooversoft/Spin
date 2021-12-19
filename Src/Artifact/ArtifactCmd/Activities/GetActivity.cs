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
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<GetActivity> _logger;

        public GetActivity(IArtifactClient artifactClient, ILogger<GetActivity> logger)
        {
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task Get(string id, string file, CancellationToken token)
        {
            id.VerifyNotEmpty(nameof(id));
            file.VerifyNotEmpty(nameof(file));

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Get), Id = id });

            _logger.LogInformation($"Reading artifact id={id} and saving to {file}...");

            ArtifactId artifactId = new ArtifactId(id);
            ArtifactPayload? payload = await _artifactClient.Get(artifactId, token);
            if (payload == null)
            {
                _logger.LogError($"Artifact {artifactId} does not exist");
                return;
            }

            byte[] bytes = payload.PayloadToBytes();
            File.WriteAllBytes(file, bytes);

            _logger.LogInformation($"Completed download {file} from artifact id={id}");
        }
    }
}
