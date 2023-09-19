using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Storage;

public class StorageClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<StorageClient> _logger;

    public StorageClient(HttpClient client, ILogger<StorageClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string storageId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}/{Uri.EscapeDataString(storageId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string storageId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}/{Uri.EscapeDataString(storageId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<StorageBlob>> Get(string storageId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}/{Uri.EscapeDataString(storageId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<StorageBlob>();

    public async Task<Option> Set(StorageBlob content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();
}
