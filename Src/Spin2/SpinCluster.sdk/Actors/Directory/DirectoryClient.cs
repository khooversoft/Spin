using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

public class DirectoryClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<DirectoryClient> _logger;

    public DirectoryClient(HttpClient client, ILogger<DirectoryClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> AddEdge(DirectoryEdge edge, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/addEdge")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(edge)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> AddNode(DirectoryNode node, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/addNode")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(node)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<DirectoryResponse>> Query(DirectoryQuery query, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/query")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(query)
        .PostAsync(context.With(_logger))
        .GetContent<DirectoryResponse>();

    public async Task<Option<DirectoryResponse>> Remove(DirectoryQuery query, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/remove")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(query)
        .PostAsync(context.With(_logger))
        .GetContent<DirectoryResponse>();

    public async Task<Option> UpdateEdge(DirectoryEdgeUpdate model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/updateEdge")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> UpdateNode(DirectoryNodeUpdate model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/updateNode")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();
}
