using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Graph;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class DirectoryClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<DirectoryClient> _logger;

    public DirectoryClient(HttpClient client, ILogger<DirectoryClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<QueryBatchResult>> Execute(string query, ScopeContext context) => await Execute(new DirectoryCommand(query), context);

    public async Task<Option<QueryBatchResult>> Execute(DirectoryCommand query, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/command")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(query)
        .PostAsync(context.With(_logger))
        .GetContent<QueryBatchResult>();

    public async Task<Option> Clear(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Directory}/{Uri.EscapeDataString(principalId)}/clear")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();
}
