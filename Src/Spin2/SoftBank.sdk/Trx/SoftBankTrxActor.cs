using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinCluster.sdk.Actors;
using Toolbox.Extensions;
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

        Direction dir = IsSourceOrDestination(request);

        var test = new OptionTest()
            .Test(() => request.Validate())
            .Test(() => dir != Direction.Invalid ? StatusCode.OK : new Option(StatusCode.BadRequest, "Request is invalid based on actor key"));
        if (test.IsError()) return test.Option.ToOptionStatus<TrxResponse>();

        return dir switch
        {
            Direction.IsSource => await ProcessAsSource(request, context),
            Direction.IsDestination => await ProcessAsDestination(request, context),

            _ => StatusCode.BadRequest,
        };
    }

    private async Task<Option<TrxResponse>> ProcessAsSource(TrxRequest request, ScopeContext context)
    {
        context.Location().LogInformation("ProcessAsSource, requestId={requestId}, type={type}", request.Id, request.Type);

        AmountReserved? amountReserved = null;

        if (request.Type == TrxType.Push)
        {
            Option<AmountReserved> reserveAmountOption = await ReserveFromSource(request, context);
            if (reserveAmountOption.IsError()) return reserveAmountOption.ToOptionStatus<TrxResponse>();
            amountReserved = reserveAmountOption.Return();
        }

        var trxResponseOption = await Forward(request, context);
        if (trxResponseOption.IsError()) return trxResponseOption;

        var ledgerItem = new LedgerItem
        {
            AccountId = request.AccountID,
            PartyAccountId = request.PartyAccountId,
            OwnerId = request.PrincipalId,
            Description = request.Description,
            Type = request.Type switch
            {
                TrxType.Push => LedgerType.Debit,
                TrxType.Pull => LedgerType.Credit,
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

        var update = await _clusterClient.GetSoftBankActor(request.AccountID).AddLedgerItem(ledgerItem, context.TraceId);
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

        AmountReserved? amountReserved = null;

        if (request.Type == TrxType.Pull)
        {
            Option<AmountReserved> reserveAmountOption = await ReserveFromDestination(request, context);
            if (reserveAmountOption.IsError()) return reserveAmountOption.ToOptionStatus<TrxResponse>();
            amountReserved = reserveAmountOption.Return();
        }

        var ledgerItem = new LedgerItem
        {
            AccountId = request.PartyAccountId,
            PartyAccountId = request.AccountID,
            OwnerId = request.PrincipalId,
            Description = request.Description,
            Type = request.Type switch
            {
                TrxType.Push => LedgerType.Credit,
                TrxType.Pull => LedgerType.Debit,
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

        var update = await _clusterClient.GetSoftBankActor(request.PartyAccountId).AddLedgerItem(ledgerItem, context.TraceId);
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

    private async Task<Option<AmountReserved>> ReserveFromSource(TrxRequest request, ScopeContext context) =>
        await Reserve(request, request.AccountID, context);

    private async Task<Option<AmountReserved>> ReserveFromDestination(TrxRequest request, ScopeContext context) =>
        await Reserve(request, request.PartyAccountId, context);

    private async Task<Option> ReleaseReservce(AmountReserved amountReserved, ScopeContext context)
    {
        context.Location().LogInformation("Releasing reserveration, accountId={accountId}, leaseKey={leaseKey}",
            amountReserved.AccountId, amountReserved.LeaseKey);

        var response = await _clusterClient
            .GetSoftBankActor(amountReserved.AccountId)
            .ReleaseReserve(amountReserved.LeaseKey, context.TraceId);

        return response;
    }

    private async Task<Option<AmountReserved>> Reserve(TrxRequest request, string accountId, ScopeContext context)
    {
        context.Location().LogInformation("Reserving funds, requestId={requestId}, accountId={accountId}", request.Id, accountId);

        Option<AmountReserved> reserveAmount = await _clusterClient
            .GetSoftBankActor(accountId)
            .Reserve(request.PrincipalId, request.Amount, context.TraceId);

        var test = new OptionTest().Test(() => reserveAmount.IsOk()).Test(() => request.Validate());
        if (test.IsError()) return test.Option.ToOptionStatus<AmountReserved>();

        context.Location().LogInformation("Lease acquired, actorKey={actorKey}, accountId={accountId}, leaseKey={leaseKey}",
            this.GetPrimaryKeyString(), accountId, reserveAmount.Return().LeaseKey);

        return reserveAmount;
    }
}
