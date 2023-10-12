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

    public async Task<Option<DirectoryResponse>> Lookup(DirectorySearch search, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/search")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(search)
        .PostAsync(context.With(_logger))
        .GetContent<DirectoryResponse>();

    public async Task<Option> RemoveEdge(string nodeKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/{Uri.EscapeDataString(nodeKey)}/edge")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> RemoveEdge(DirectoryEdge edge, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/edge")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(edge)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> RemoveNode(string nodeKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/{Uri.EscapeDataString(nodeKey)}/node")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();


}
