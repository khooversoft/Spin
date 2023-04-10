using ArtifactCmd.Application;
using Artifact.sdk.Client;
using Artifact.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Tools;

namespace ArtifactCmd.Activities
{
    internal class SetActivity
    {
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<SetActivity> _logger;

        public SetActivity(IArtifactClient artifactClient, ILogger<SetActivity> logger)
        {
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task Set(string file, string id, CancellationToken token)
        {
            file.VerifyNotEmpty(nameof(file));
            id.VerifyNotEmpty(nameof(id));

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Set), File = file, Id = id });

            _logger.LogInformation($"Writing {file} to artifact id={id}...");

            byte[] bytes = await File.ReadAllBytesAsync(file, token);

            ArtifactPayload payload = new ArtifactPayloadBuilder()
                .SetId((ArtifactId)id)
                .SetPayload(bytes)
                .Build();

            await _artifactClient.Set(payload, token);

            _logger.LogInformation($"Completed writing {file} to artifact id={id}.");
        }
    }
}
