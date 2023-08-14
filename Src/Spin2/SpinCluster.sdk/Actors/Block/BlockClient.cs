﻿using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Block;

public class BlockClient
{
    protected readonly HttpClient _client;
    public BlockClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(ObjectId objectId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.BlockStorage}/{objectId.ToUrlEncoding()}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<BlobPackage>> Get(ObjectId objectId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.BlockStorage}/{objectId.ToUrlEncoding()}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<BlobPackage>();

    public async Task<Option> Set(BlobPackage content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.BlockStorage}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
