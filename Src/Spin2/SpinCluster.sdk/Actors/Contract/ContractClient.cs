﻿using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

public class ContractClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<ContractClient> _logger;

    public ContractClient(HttpClient client, ILogger<ContractClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string documentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string documentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Create(ContractCreateModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<IReadOnlyList<DataBlock>>> Query(string documentId, ContractQuery content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/query")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .GetContent<IReadOnlyList<DataBlock>>();

    public async Task<Option> Append(string documentId, DataBlock content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/append")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<ContractPropertyModel>> GetProperties(string documentId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Contract}/{Uri.EscapeDataString(documentId)}/property")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context.With(_logger))
        .GetContent<ContractPropertyModel>();
}
