using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Lease;
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

    public async Task<Option<IReadOnlyList<LeaseData>>> GetActiveReservations(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!IdPatterns.IsPrincipalId(principalId)) return new(StatusCode.BadRequest, "Invalid principalId");
        context.Location().LogInformation("Getting activce leases for trx, principalId={principalId}", principalId);

        var query = new QueryParameter { Filter = _parent.GetPrimaryKeyString() };
        var listOption = await _clusterClient.GetLeaseActor().List(query, context.TraceId);
        return listOption;
    }

    public async Task<Option<SbAccountBalance>> GetReserveBalance(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Reserve funds for trx, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return new(StatusCode.BadRequest, "Invalid principalId");

        var leaseDataOption = await GetActiveReservations(principalId, context.TraceId);
        if (leaseDataOption.IsError()) return leaseDataOption.ToOptionStatus<SbAccountBalance>();

        var reserveTotal = leaseDataOption.Return()
            .Where(x => x.Payload != null)
            .Select(x => x.Payload.NotNull().ToObject<ReserveLease>().NotNull())
            .Sum(x => x.Amount);

        var response = new SbAccountBalance
        {
            DocumentId = _parent.GetPrimaryKeyString(),
            ReserveBalance = reserveTotal,
        };

        return response;
    }

    public async Task<Option<SbAmountReserved>> Reserve(string principalId, decimal amount, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Reserve funds for trx, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return new(StatusCode.BadRequest, "Invalid principalId");
        if (amount <= 0) return new(StatusCode.BadRequest, "Amount is lessthan zero");

        var hasAccess = await _parent.GetContractActor().HasAccess(principalId, BlockGrant.Write, typeof(SbLedgerItem).GetTypeName(), traceId);
        if (hasAccess.IsError()) return hasAccess.ToOptionStatus<SbAmountReserved>();

        // Verify money is available
        SbAccountBalance balance = await _parent.GetBalance(principalId, traceId).Return();
        if (balance.PrincipalBalance < amount) return new(StatusCode.BadRequest, "No funds");

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

    private LeaseData CreateLeaseRequest(decimal amount) => new LeaseData
    {
        LeaseKey = new[]
        {
            "softbank",
            _parent.GetPrimaryKeyString(),
            $"ReserveAmount/{Guid.NewGuid()}"
        }.Join('/'),
        Payload = new ReserveLease { Amount = amount }.ToJson(),
        Reference = _parent.GetPrimaryKeyString(),
    };

    internal sealed record ReserveLease
    {
        public decimal Amount { get; init; }
    }
}
