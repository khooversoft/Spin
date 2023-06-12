using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DocumentObjectLease
{
    private readonly ITimeContext _timeContext;
    private readonly ILogger<DocumentObjectLease> _logger;
    private ConcurrentDictionary<string, Lease> _leases = new(StringComparer.OrdinalIgnoreCase);

    public DocumentObjectLease(ITimeContext timeContext, ILogger<DocumentObjectLease> logger)
    {
        _timeContext = timeContext;
        _logger = logger;
    }

    public string GetLease(DateTime validTo, string objectId, decimal amount, ScopeContext context)
    {
        objectId.NotNull();

        var model = new Lease
        {
            ValidTo = validTo,
            ObjectId = objectId,
            Amount = amount
        };

        _leases.TryAdd(model.Id, model);
        context.Location().LogInformation("Issuing lease to leaseTo={leaseTo}, amount={amount}, validTo={validTo}", objectId, amount, validTo);

        return model.Id;
    }

    public string GetLease(TimeSpan validFor, string objectId, decimal amount, ScopeContext context) =>
        GetLease(DateTime.UtcNow + validFor, objectId, amount, context);

    public bool IsLeased(string id, string? leaseId = null)
    {
        bool found = _leases.TryGetValue(id, out Lease? lease);
        if (!found || lease == null) return false;

        if (lease.ValidTo > _timeContext.GetUtc())
        {
            _leases.TryRemove(id, out var _);
            return false;
        }

        return leaseId switch
        {
            null => true,
            string v when v == lease.Id => true,
            _ => false,
        };
    }

    public bool ReleaseLease(string id, ScopeContext context)
    {
        _leases.TryRemove(id, out var _);
        context.Location().LogInformation("Release lease leaseTo={id}", id);

        return true;
    }

    private record Lease
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public DateTime ValidTo { get; init; } = DateTime.UtcNow;
        public string ObjectId { get; init; } = null!;
        public decimal Amount { get; init; }
    }
}
