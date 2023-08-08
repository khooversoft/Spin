using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using SoftBank.sdk.Models;
using Microsoft.AspNetCore.Http;
using Toolbox.Block;

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
        var group = app.MapGroup($"/{SpinConstants.Schema.SoftBank}");

        group.MapPost("{*objectId}", Create);
        group.MapDelete("{*objectId}", Delete);
        group.MapGet("exist/{*objectId}", Exist);
        group.MapGet("accountDetail/{*objectId}", GetAccountDetail);
        group.MapPost("accountDetail/{*objectId}", SetAccountDetail);
        group.MapGet("balance/{*objectId}", GetBalance);
        group.MapPost("acl/{*objectId}", SetAcl);
        group.MapGet("ledgerItem/{*objectId}", GetLedgerItems);
        group.MapPost("ledgerItem/{*objectId}", AddLedgerItem);

        app.MapGroup($"/{SpinConstants.Schema.SoftBank}/ledgerItem").Action(x =>
        {
            x.MapPost("{*objectId}", AddLedgerItem);
        });
    }

    private async Task<IResult> Create(string objectId, AccountDetail model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        Option response = await _client.GetGrain<ISoftBankActor>(option.Return()).Create(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Delete(string objectId, 
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId
        )
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        Option response = await _client.GetGrain<ISoftBankActor>(option.Return()).Delete(principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string objectId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetGrain<ISoftBankActor>(option.Return()).Exist(traceId);
        return response.ToResult();
    }

    private async Task<IResult> GetAccountDetail(string objectId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId
        )
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        Option<AccountDetail> response = await _client.GetGrain<ISoftBankActor>(option.Return()).GetAccountDetail(principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> SetAccountDetail(string objectId, AccountDetail model,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId
        )
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        Option response = await _client.GetGrain<ISoftBankActor>(option.Return()).SetAccountDetail(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> GetBalance(string objectId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId
        )
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        Option<AccountBalance> response = await _client.GetGrain<ISoftBankActor>(option.Return()).GetBalance(principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> SetAcl(string objectId, BlockAcl model,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId
    )
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        Option response = await _client.GetGrain<ISoftBankActor>(option.Return()).SetAcl(model, principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> GetLedgerItems(string objectId,
        [FromHeader(Name = SpinConstants.Headers.PrincipalId)] string principalId,
        [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId
    )
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetGrain<ISoftBankActor>(option.Return()).GetLedgerItems(principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> AddLedgerItem(string objectId, LedgerItem model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetGrain<ISoftBankActor>(option.Return()).AddLedgerItem(model, traceId);
        return response.ToResult();
    }
}
