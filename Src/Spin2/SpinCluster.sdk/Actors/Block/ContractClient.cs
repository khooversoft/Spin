﻿using System.Reflection.Metadata;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Block;

public class ContractClient
{
    protected readonly HttpClient _client;
    public ContractClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string documentId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option> Exist(string documentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();

    public async Task<Option> Create(ContractCreateModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option<DataBlock>> GetLatest(string documentId, string blockType, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/latest/{Uri.EscapeDataString(blockType)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<DataBlock>();

    public async Task<Option<IReadOnlyList<DataBlock>>> List(string documentId, string blockType, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/list/{Uri.EscapeDataString(blockType)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<IReadOnlyList<DataBlock>>();

    public async Task<Option> Append(string documentId, DataBlock content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/append")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

}
