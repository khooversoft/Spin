using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.DocumentContainer;

public class DocumentLease
{
    private readonly ITimeContext _timeContext;
    private readonly ILogger<DocumentLease> _logger;
    private ConcurrentDictionary<string, Lease> _leases = new(StringComparer.OrdinalIgnoreCase);

    public DocumentLease(ITimeContext timeContext, ILogger<DocumentLease> logger)
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
        _logger.LogInformation(context.Location(), "Issuing lease to leaseTo={leaseTo}, amount={amount}, validTo={validTo}", objectId, amount, validTo);

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
        _logger.LogInformation(context.Location(), "Release lease leaseTo={id}", id);

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
