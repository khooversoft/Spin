using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
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
        group.MapPost("/{documentId}/query", Query);
        group.MapPost("/{documentId}/append", Append);
        group.MapGet("/{documentId}/property", Property);

        return group;
    }

    private async Task<IResult> Delete(string documentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        documentId = Uri.UnescapeDataString(documentId);
        if (!IdPatterns.IsContractId(documentId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IContractActor>(documentId).Delete(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string documentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        documentId = Uri.UnescapeDataString(documentId);
        if (!IdPatterns.IsContractId(documentId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IContractActor>(documentId).Exist(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Create(ContractCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client.GetResourceGrain<IContractActor>(model.DocumentId).Create(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Query(string documentId, ContractQuery model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        documentId = Uri.UnescapeDataString(documentId);
        if (!IdPatterns.IsContractId(documentId)) return Results.BadRequest();
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option<ContractQueryResponse> response = await _client.GetResourceGrain<IContractActor>(documentId).Query(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Append(string documentId, DataBlock model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        documentId = Uri.UnescapeDataString(documentId);
        if (!IdPatterns.IsContractId(documentId)) return Results.BadRequest();
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client.GetResourceGrain<IContractActor>(documentId).Append(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Property(
        string documentId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        documentId = Uri.UnescapeDataString(documentId);
        if (!IdPatterns.IsContractId(documentId)) return Results.BadRequest("Invalid documentId");
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest("Invalid principalId");

        Option<ContractPropertyModel> response = await _client.GetResourceGrain<IContractActor>(documentId).GetProperties(principalId, traceId);
        return response.ToResult();
    }
}
