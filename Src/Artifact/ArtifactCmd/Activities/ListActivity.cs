using ArtifactCmd.Application;
using ArtifactStore.sdk.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace ArtifactCmd.Activities
{
    internal class ListActivity
    {
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<ListActivity> _logger;

        public ListActivity(IArtifactClient artifactClient, ILogger<ListActivity> logger)
        {
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task List(string nameSpace, CancellationToken token)
        {
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(List), Namespace = nameSpace });

            BatchSetCursor<string> batch = _artifactClient.List(null!);
            int index = 0;

            var list = new List<string>
            {
                $"Listing artifacts from Namespace {nameSpace}...",
                "",
            };

            while (true)
            {
                BatchSet<string> batchSet = await batch.ReadNext(token);
                if (batchSet.IsEndSignaled) break;

                batchSet.Records.ForEach(x => list.Add($"({index++}) {nameSpace + "/" + x}"));
            }

            list.Add($"Completed, {index} listed");

            _logger.LogInformation(list.Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine));
        }
    }
}
