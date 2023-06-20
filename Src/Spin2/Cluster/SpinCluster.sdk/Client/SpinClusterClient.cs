using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Client;

public class SpinClusterClient
{
    private readonly HttpClient _client;
    private readonly ILogger<SpinClusterClient> _logger;

    public SpinClusterClient(HttpClient client, ILogger<SpinClusterClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<StatusCode> Delete(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath(id)
        .AddHeader(SpinClusterConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetStatusCode();

    public async Task<Option<T>> Get<T>(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath(id)
        .AddHeader(SpinClusterConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<T>();

    public async Task<StatusCode> Set<T>(ObjectId id, T content, ScopeContext context) => await new RestClient(_client)
        .SetPath(id)
        .AddHeader(SpinClusterConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .GetStatusCode();
}

