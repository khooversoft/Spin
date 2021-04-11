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
        private readonly Option _option;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<ListActivity> _logger;

        public ListActivity(Option option, IArtifactClient artifactClient, ILogger<ListActivity> logger)
        {
            _option = option;
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task List(CancellationToken token)
        {
            BatchSetCursor<string> batch = _artifactClient.List(new QueryParameter { Namespace = _option.Namespace });
            int index = 0;

            _logger.LogInformation("Listing artifacts...");

            while (true)
            {
                BatchSet<string> batchSet = await batch.ReadNext(token);
                if (batchSet.Records.Count == 0) break;

                batchSet.Records.ForEach(x => _logger.LogInformation($"({index++}) {x}"));
            }

            _logger.LogInformation($"Completed, {index} listed");
        }
    }
}
