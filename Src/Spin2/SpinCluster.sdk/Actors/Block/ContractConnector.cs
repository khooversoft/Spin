using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using Toolbox.Block;
using Azure.Storage.Blobs.Models;
using System.Reflection.Metadata;

namespace SpinCluster.sdk.Actors.Block;

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
        group.MapPost("/{documentId}/create", Create);
        group.MapGet("/{documentId}/latest/{blockType}", GetLatest);
        group.MapGet("/{documentId}/list/{blockType}", List);
        group.MapPost("/{documentId}/append", Append);

        return group;
    }

    private async Task<IResult> Delete(
        string documentId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        documentId = Uri.UnescapeDataString(documentId);

        var test = new Option()
            .Test(() => IdPatterns.IsContractId(documentId))
            .Test(() => IdPatterns.IsPrincipalId(principalId));
        if (test.IsError()) return test.ToResult();

        Option response = await _client.GetContractActor(documentId).Delete(principalId, traceId);
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

    private async Task<IResult> GetLatest(
        string documentId, 
        string blockType, 
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        var test = new Option()
            .Test(() => IdPatterns.IsContractId(documentId))
            .Test(() => IdPatterns.IsPrincipalId(principalId))
            .Test(() => IdPatterns.IsBlockType(blockType));
        if (test.IsError()) return test.ToResult();

        Option<DataBlock> response = await _client.GetContractActor(documentId).GetLatest(blockType, principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> List(
        string documentId,
        string blockType,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        var test = new Option()
            .Test(() => IdPatterns.IsContractId(documentId))
            .Test(() => IdPatterns.IsPrincipalId(principalId))
            .Test(() => IdPatterns.IsBlockType(blockType));
        if (test.IsError()) return test.ToResult();

        Option<IReadOnlyList<DataBlock>> response = await _client.GetContractActor(documentId).List(blockType, principalId, traceId);
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
