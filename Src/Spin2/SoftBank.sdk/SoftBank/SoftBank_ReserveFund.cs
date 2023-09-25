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
            .Test(() => amount > 0 ? StatusCode.OK : (StatusCode.BadRequest, "Amount must be positive"));
        if (test.IsError()) return test.Option.ToOptionStatus<SbAmountReserved>();

        var hasAccess = await _parent.GetContractActor().HasAccess(principalId, BlockGrant.Write, typeof(SbLedgerItem).GetTypeName(), traceId);
        if (hasAccess.IsError()) return hasAccess.ToOptionStatus<SbAmountReserved>();

        // Verify money is available
        SbAccountBalance balance = await _parent.GetBalance(principalId, traceId).Return();
        if (balance.PrincipalBalance < amount) return new Option<SbAmountReserved>(StatusCode.BadRequest, "No funds");

        var request = CreateLeaseRequest(amount);
        var acquire = await _clusterClient.GetLeaseActor().Acquire(request, context.TraceId);
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

        var releaseResponse = await _clusterClient.GetLeaseActor().Release(leaseKey, context.TraceId);
        return releaseResponse;
    }

    private LeaseCreate CreateLeaseRequest(decimal amount) => new LeaseCreate
    {
        LeaseKey = BuildLeaseKey(),
        Payload = new ReserveLease { Amount = amount }.ToJson(),
    };

    private string BuildLeaseKey() => new[]
    {
        "softbank",
        _parent.GetPrimaryKeyString(),
        $"ReserveAmount/{Guid.NewGuid()}"
    }.Join('/');

    internal sealed record ReserveLease
    {
        public decimal Amount { get; init; }
    }
}
