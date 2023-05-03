//using System.Collections.Concurrent;
//using Microsoft.Extensions.Logging;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Block.Test.Scenarios;

//public class LeaseCollection
//{
//    private readonly ITimeContext _timeContext;
//    private readonly ILogger<LeaseCollection> _logger;
//    private ConcurrentDictionary<string, Lease> _leases = new(StringComparer.OrdinalIgnoreCase);

//    public LeaseCollection(ITimeContext timeContext, ILogger<LeaseCollection> logger)
//    {
//        _timeContext = timeContext;
//        _logger = logger;
//    }

//    public string GetLease(DateTime validTo, string leaseTo, decimal amount, ScopeContext context)
//    {
//        leaseTo.NotNull();

//        var model = new Lease
//        {
//            ValidTo = validTo,
//            LeaseTo = leaseTo,
//            Amount = amount
//        };

//        _leases.TryAdd(model.Id, model);
//        _logger.LogInformation(context.Location(), "Issuing lease to leaseTo={leaseTo}, amount={amount}, validTo={validTo}", leaseTo, amount, validTo);

//        return model.Id;
//    }

//    public string GetLease(TimeSpan validFor, string leaseTo, decimal amount, ScopeContext context) =>
//        GetLease(DateTime.UtcNow + validFor, leaseTo, amount, context);

//    public bool ReleaseLease(string id, ScopeContext context)
//    {
//        bool found = _leases.TryRemove(id, out Lease? lease);
//        if (!found || lease == null || lease.ValidTo > _timeContext.GetUtc()) return false;

//        _logger.LogInformation(context.Location(), "Release lease leaseTo={id}", id);
//        return found;
//    }

//    private record Lease
//    {
//        public string Id { get; init; } = Guid.NewGuid().ToString();
//        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
//        public DateTime ValidTo { get; init; } = DateTime.UtcNow;
//        public string LeaseTo { get; init; } = null!;
//        public decimal Amount { get; init; }
//    }
//}
