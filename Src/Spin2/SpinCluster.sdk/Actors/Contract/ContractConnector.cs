﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

public class ContractConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<ContractConnector> _logger;

    public ContractConnector(IClusterClient client, ILogger<ContractConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Contract}");

        group.MapDelete("/{documentId}", Delete);
        group.MapGet("/{documentId}/exist", Exist);
        group.MapPost("/create", Create);
        group.MapPost("/query", Query);
        group.MapPost("/{documentId}/append", Append);

        return group;
    }

    private async Task<IResult> Delete(
        string documentId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId
        )
    {
        documentId = Uri.UnescapeDataString(documentId);
        if (!IdPatterns.IsContractId(documentId)) return Results.BadRequest();

        Option response = await _client.GetContractActor(documentId).Delete(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string documentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        documentId = Uri.UnescapeDataString(documentId);
        if (!IdPatterns.IsContractId(documentId)) return Results.BadRequest();

        Option response = await _client.GetContractActor(documentId).Exist(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Create(ContractCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (model.Validate().IsError()) return Results.BadRequest();

        Option response = await _client.GetContractActor(model.DocumentId).Create(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Query(ContractQuery model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var v = model.Validate();
        if (v.IsError()) return v.ToResult();

        Option<IReadOnlyList<DataBlock>> response = await _client.GetContractActor(model.DocumentId).Query(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Append(string documentId, DataBlock content, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var test = new Option()
            .Test(() => IdPatterns.IsContractId(documentId))
            .Test(() => content.Validate());
        if (test.IsError()) return test.ToResult();

        Option response = await _client.GetContractActor(documentId).Append(content, traceId);
        return response.ToResult();
    }
}