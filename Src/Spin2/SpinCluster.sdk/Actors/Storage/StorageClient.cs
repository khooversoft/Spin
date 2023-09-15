﻿using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Storage;

public class StorageClient
{
    protected readonly HttpClient _client;
    public StorageClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string storageId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}/{Uri.EscapeDataString(storageId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option> Exist(string storageId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}/{Uri.EscapeDataString(storageId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<StorageBlob>> Get(string storageId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}/{Uri.EscapeDataString(storageId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<StorageBlob>();

    public async Task<Option> Set(StorageBlob content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Storage}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
