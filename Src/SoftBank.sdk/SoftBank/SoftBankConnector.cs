﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.SoftBank;

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
        var group = app.MapGroup($"/{IdSoftbank.SoftBankSchema}");

        group.MapDelete("/{accountId}", Delete);
        group.MapGet("/{accountId}/exist", Exist);
        group.MapPost("/create", Create);
        group.MapPost("/{accountId}/accountdetail", SetAccountDetail);
        group.MapPost("/{accountId}/acl", SetAcl);
        group.MapPost("/{accountId}/ledgerItem", AddLedgerItem);
        group.MapGet("/{accountId}/accountDetail", GetAccountDetail);
        group.MapGet("/{accountId}/ledgerItem", GetLedgerItems);
        group.MapGet("/{accountId}/balance", GetBalance);
    }

    private async Task<IResult> Delete(string accountId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISoftBankActor>(accountId).Delete(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string accountId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        var response = await _client.GetResourceGrain<ISoftBankActor>(accountId).Exist(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Create(SbAccountDetail model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var v = model.Validate();
        if (v.IsError()) return v.ToResult();

        Option response = await _client.GetResourceGrain<ISoftBankActor>(model.AccountId).Create(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> SetAccountDetail(string accountId, SbAccountDetail model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISoftBankActor>(accountId).SetAccountDetail(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> SetAcl(string accountId, AclBlock model,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISoftBankActor>(accountId).SetAcl(model, principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> AddLedgerItem(string accountId, SbLedgerItem model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISoftBankActor>(accountId).AddLedgerItem(model, traceId);
        return response.ToResult();
    }

    public async Task<IResult> GetAccountDetail(string accountId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        Option<SbAccountDetail> response = await _client.GetResourceGrain<ISoftBankActor>(accountId).GetAccountDetail(principalId, traceId);
        return response.ToResult();
    }

    public async Task<IResult> GetLedgerItems(string accountId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        Option<IReadOnlyList<SbLedgerItem>> response = await _client.GetResourceGrain<ISoftBankActor>(accountId).GetLedgerItems(principalId, traceId);
        return response.ToResult();
    }

    public async Task<IResult> GetBalance(string accountId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId
        )
    {
        accountId = Uri.UnescapeDataString(accountId);
        if (!IdSoftbank.IsSoftBankId(accountId)) return Results.BadRequest();

        Option<SbAccountBalance> response = await _client.GetResourceGrain<ISoftBankActor>(accountId).GetBalance(principalId, traceId);
        return response.ToResult();
    }
}
