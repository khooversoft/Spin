using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.SoftBank;

internal class SoftBank_ReserveFund
{
    private readonly SoftBankActor _parent;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger _logger;

    public SoftBank_ReserveFund(SoftBankActor parent, IClusterClient clusterClient, ILogger logger)
    {
        _parent = parent;
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task<Option<SbAmountReserved>> Reserve(string principalId, decimal amount, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Reserve funds for trx, principalId={principalId}", principalId);

        var test = new OptionTest()
            .Test(() => IdPatterns.IsPrincipalId(principalId))
            .Test(() => amount > 0 ? StatusCode.OK : new Option(StatusCode.BadRequest, "Amount must be positive"));
        if (test.IsError()) return test.Option.ToOptionStatus<SbAmountReserved>();

        var hasAccess = await _parent.GetContractActor().HasAccess(principalId, BlockGrant.Write, typeof(SbLedgerItem).GetTypeName(), traceId);
        if (hasAccess.IsError()) return hasAccess.ToOptionStatus<SbAmountReserved>();

        // Verify money is available
        SbAccountBalance balance = await _parent.GetBalance(principalId, traceId).Return();
        if (balance.Balance < amount) return new Option<SbAmountReserved>(StatusCode.BadRequest, "No funds");

        ILeaseActor leaseActor = GetLeaseActor();

        var request = CreateLeaseRequest(amount);
        var acquire = await leaseActor.Acquire(request, context.TraceId);
        if (acquire.IsError()) return acquire.ToOptionStatus<SbAmountReserved>();

        var reserved = new SbAmountReserved
        {
            LeaseKey = request.LeaseKey,
            AccountId = _parent.GetPrimaryKeyString(),
            PrincipalId = principalId,
            Amount = amount,
            GoodTo = DateTime.UtcNow + request.TimeToLive,
        };

        return reserved;
    }

    public async Task<Option> ReleaseReserve(string leaseKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Release reserve funds for trx, leaseKey={leaseKey}", leaseKey);

        ILeaseActor leaseActor = GetLeaseActor();
        var releaseResponse = await leaseActor.Release(leaseKey, context.TraceId);

        return releaseResponse;
    }

    internal ILeaseActor GetLeaseActor() => ResourceId.Create(_parent.GetPrimaryKeyString()).ThrowOnError()
        .Bind(x => IdTool.CreateLeaseId(x.Domain.NotNull(), "softbank/" + x.Path.NotNull()))
        .Bind(x => _clusterClient.GetResourceGrain<ILeaseActor>(x))
        .Return();

    private static LeaseCreate CreateLeaseRequest(decimal amount) => new LeaseCreate
    {
        LeaseKey = ReserveLease.BuildLeaseKey(amount),
        Payload = new ReserveLease { Amount = amount }.ToJson(),
    };

    internal sealed record ReserveLease
    {
        public static string BuildLeaseKey(decimal amount) => $"ReserveAmount/{Guid.NewGuid()}";
        public decimal Amount { get; init; }
    }
}
