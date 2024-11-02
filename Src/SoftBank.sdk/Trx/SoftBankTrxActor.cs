using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinCluster.sdk.Actors;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Trx;


public interface ISoftBankTrxActor : IGrainWithStringKey
{
    Task<Option<TrxResponse>> Request(TrxRequest request, string traceId);
}


public class SoftBankTrxActor : Grain, ISoftBankTrxActor
{
    private readonly ILogger<SoftBankActor> _logger;
    private readonly IClusterClient _clusterClient;

    private enum Direction { Invalid, IsSource, IsDestination }

    public SoftBankTrxActor(IClusterClient clusterClient, ILogger<SoftBankActor> logger)
    {
        _logger = logger.NotNull();
        _clusterClient = clusterClient.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema("softbank-trx", new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<TrxResponse>> Request(TrxRequest request, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!request.Validate(out var v)) return v.ToOptionStatus<TrxResponse>();

        var result = IsSourceOrDestination(request) switch
        {
            Direction.IsSource => await ProcessAsSource(request, context),
            Direction.IsDestination => await ProcessAsDestination(request, context),

            _ => (StatusCode.BadRequest, "Request is invalid based on actor key"),
        };

        result.LogStatus(context, "Request processed");
        return result;
    }

    private async Task<Option<TrxResponse>> ProcessAsSource(TrxRequest request, ScopeContext context)
    {
        context.Location().LogInformation("ProcessAsSource, requestId={requestId}, type={type}", request.Id, request.Type);

        SbAmountReserved? amountReserved = null;

        if (request.Type == TrxType.Push)
        {
            Option<SbAmountReserved> reserveAmountOption = await ReserveFromSource(request, context);
            if (reserveAmountOption.IsError()) return reserveAmountOption.ToOptionStatus<TrxResponse>();
            amountReserved = reserveAmountOption.Return();
        }

        var trxResponseOption = await Forward(request, context);
        if (trxResponseOption.IsError()) return trxResponseOption;

        var ledgerItem = new SbLedgerItem
        {
            AccountId = request.AccountID,
            PartyAccountId = request.PartyAccountId,
            OwnerId = request.PrincipalId,
            Description = request.Description,
            Type = request.Type switch
            {
                TrxType.Push => SbLedgerType.Debit,
                TrxType.Pull => SbLedgerType.Credit,
                _ => throw new UnreachableException(),
            },
            Amount = request.Amount,
            Tags = new[]
            {
                "trxPush",
                $"Request:Id={request.Id}",
                $"Request:Description={request.Description}",
                $"DestinationAccountId={request.PartyAccountId}",
            }.Join(';'),
        };

        //var update = await _clusterClient.GetSoftBankActor(request.AccountID).AddLedgerItem(ledgerItem, context.TraceId);
        var update = await TryAddingToledger(ledgerItem, context);
        if (update.IsError())
        {
            context.Location().LogCritical("Failed to add ledger item");
            return update.ToOptionStatus<TrxResponse>();
        }

        if (amountReserved != null) await ReleaseReservce(amountReserved, context);

        TrxResponse trxResponse = trxResponseOption.Return() with
        {
            Status = TrxStatusCode.Completed,
            SourceLedgerItemId = ledgerItem.Id
        };
        return trxResponse;
    }

    private async Task<Option<TrxResponse>> ProcessAsDestination(TrxRequest request, ScopeContext context)
    {
        context.Location().LogInformation("ProcessAsDestination, requestId={requestId}, type={type}", request.Id, request.Type);

        SbAmountReserved? amountReserved = null;

        if (request.Type == TrxType.Pull)
        {
            Option<SbAmountReserved> reserveAmountOption = await ReserveFromDestination(request, context);
            if (reserveAmountOption.IsError()) return reserveAmountOption.ToOptionStatus<TrxResponse>();
            amountReserved = reserveAmountOption.Return();
        }

        var ledgerItem = new SbLedgerItem
        {
            AccountId = request.PartyAccountId,
            PartyAccountId = request.AccountID,
            OwnerId = request.PrincipalId,
            Description = request.Description,
            Type = request.Type switch
            {
                TrxType.Push => SbLedgerType.Credit,
                TrxType.Pull => SbLedgerType.Debit,
                _ => throw new UnreachableException(),
            },
            Amount = request.Amount,
            Tags = new[]
            {
                "trxPush",
                $"Request:Id={request.Id}",
                $"Request:Description={request.Description}",
                $"DestinationAccountId={request.PartyAccountId}",
            }.Join(';'),
        };

        var update = await TryAddingToledger(ledgerItem, context);
        //var update = await _clusterClient.GetSoftBankActor(request.PartyAccountId).AddLedgerItem(ledgerItem, context.TraceId);
        if (update.IsError())
        {
            context.Location().LogCritical("Failed to add ledger item");
            return update.ToOptionStatus<TrxResponse>();
        }

        if (amountReserved != null) await ReleaseReservce(amountReserved, context);

        var trxResponse = new TrxResponse
        {
            Request = request,
            Amount = request.Amount,
            DestinationLedgerItemId = ledgerItem.Id,
        };

        return trxResponse;
    }

    private async Task<Option> TryAddingToledger(SbLedgerItem ledgerItem, ScopeContext context)
    {
        var update = await _clusterClient.GetSoftBankActor(ledgerItem.AccountId).AddLedgerItem(ledgerItem, context.TraceId);
        if (update.IsOk()) return update;

        ledgerItem = ledgerItem with { OwnerId = SoftBankConstants.SoftBankPrincipalId };

        update = await _clusterClient.GetSoftBankActor(ledgerItem.AccountId).AddLedgerItem(ledgerItem, context.TraceId);
        if (update.IsError()) return update;
        return update;
    }

    private Direction IsSourceOrDestination(TrxRequest request)
    {
        var actorId = (ResourceId)this.GetPrimaryKeyString();
        var sourceId = (ResourceId)request.AccountID;
        var destinationId = (ResourceId)request.PartyAccountId;

        return (actorId.AccountId == sourceId.AccountId, actorId.AccountId == destinationId.AccountId) switch
        {
            (true, false) => Direction.IsSource,
            (false, true) => Direction.IsDestination,
            _ => Direction.Invalid,
        };
    }

    internal ISoftBankTrxActor GetDesinationTrxActor(ResourceId desinationAccountId) =>
        _clusterClient.GetSoftBankTrxActor($"softbank-trx:{desinationAccountId.AccountId}");

    private async Task<Option<TrxResponse>> Forward(TrxRequest request, ScopeContext context) =>
        await GetDesinationTrxActor(request.PartyAccountId).Request(request, context.TraceId);

    private async Task<Option<SbAmountReserved>> ReserveFromSource(TrxRequest request, ScopeContext context) =>
        await Reserve(request, request.AccountID, context);

    private async Task<Option<SbAmountReserved>> ReserveFromDestination(TrxRequest request, ScopeContext context) =>
        await Reserve(request, request.PartyAccountId, context);

    private async Task<Option> ReleaseReservce(SbAmountReserved amountReserved, ScopeContext context)
    {
        context.Location().LogInformation("Releasing reserveration, accountId={accountId}, leaseKey={leaseKey}",
            amountReserved.AccountId, amountReserved.LeaseKey);

        var response = await _clusterClient
            .GetSoftBankActor(amountReserved.AccountId)
            .ReleaseReserve(amountReserved.LeaseKey, context.TraceId);

        return response;
    }

    private async Task<Option<SbAmountReserved>> Reserve(TrxRequest request, string accountId, ScopeContext context)
    {
        context.Location().LogInformation("Reserving funds, requestId={requestId}, accountId={accountId}", request.Id, accountId);
        if (!request.Validate(out var v)) return v.ToOptionStatus<SbAmountReserved>();
        if (!IdPatterns.IsAccountId(accountId)) return new Option<SbAmountReserved>(StatusCode.BadRequest, "Invalid accountId");

        Option<SbAmountReserved> reserveAmount = await _clusterClient.GetSoftBankActor(accountId).Reserve(request.PrincipalId, request.Amount, context.TraceId);
        if (reserveAmount.IsError()) return reserveAmount;

        context.Location().LogInformation("Lease acquired, actorKey={actorKey}, accountId={accountId}, leaseKey={leaseKey}",
            this.GetPrimaryKeyString(), accountId, reserveAmount.Return().LeaseKey);

        return reserveAmount;
    }
}
