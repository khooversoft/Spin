using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.SoftBank;

public class SoftBankConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<SoftBankConnector> _logger;

    public SoftBankConnector(IClusterClient client, ILogger<SoftBankConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual void Setup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"/softbank");

        group.MapDelete("/{accountId}", Delete);
        group.MapGet("/{accountId}/exist", Exist);
        group.MapPost("/{accountId}/create", Create);
        group.MapPost("/{accountId}/accountdetail", SetAccountDetail);
        group.MapPost("/{accountId}/acl", SetAcl);
        group.MapPost("/{accountId}/ledgerItem", AddLedgerItem);
        group.MapGet("/{accountId}/accountDetail", GetAccountDetail);
        group.MapGet("/{accountId}/ledgerItem", GetLedgerItems);
    }

    private async Task<IResult> Delete(string accountId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISoftBankActor>(accountId).Delete(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string accountId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        var response = await _client.GetGrain<ISoftBankActor>(accountId).Exist(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Create(string accountId, AccountDetail model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        Option response = await _client.GetGrain<ISoftBankActor>(accountId).Create(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> SetAccountDetail(string accountId, AccountDetail model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        Option response = await _client.GetGrain<ISoftBankActor>(accountId).SetAccountDetail(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> SetAcl(string accountId, BlockAcl model,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        Option response = await _client.GetGrain<ISoftBankActor>(accountId).SetAcl(model, principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> AddLedgerItem(string accountId, LedgerItem model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        Option response = await _client.GetGrain<ISoftBankActor>(accountId).AddLedgerItem(model, traceId);
        return response.ToResult();
    }

    public async Task<IResult> GetAccountDetail(string accountId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        Option<AccountDetail> response = await _client.GetGrain<ISoftBankActor>(accountId).GetAccountDetail(principalId, traceId);
        return response.ToResult();
    }

    public async Task<IResult> GetLedgerItems(string accountId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdPatterns.IsContractId(accountId)) return Results.BadRequest();

        Option<IReadOnlyList<LedgerItem>> response = await _client.GetGrain<ISoftBankActor>(accountId).GetLedgerItems(principalId, traceId);
        return response.ToResult();
    }
}
