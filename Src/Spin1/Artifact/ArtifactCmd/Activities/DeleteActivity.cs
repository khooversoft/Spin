using ArtifactCmd.Application;
using Artifact.sdk.Client;
using Artifact.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace ArtifactCmd.Activities;

internal class DeleteActivity
{
    private readonly IArtifactClient _artifactClient;
    private readonly ILogger<DeleteActivity> _logger;

    public DeleteActivity(IArtifactClient artifactClient, ILogger<DeleteActivity> logger)
    {
        _artifactClient = artifactClient;
        _logger = logger;
    }

    public async Task Delete(string id, CancellationToken token)
    {
        id.VerifyNotEmpty(nameof(id));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(Delete), Id = id });

        _logger.LogInformation($"Deleting {id} artifact...");

        bool removed = await _artifactClient.Delete((ArtifactId)id, token);

        _logger.LogInformation($"{(removed ? "Completed" : "Failed")} deleting {id}.");
    }
}
