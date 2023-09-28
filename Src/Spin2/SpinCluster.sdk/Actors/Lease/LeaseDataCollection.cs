using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

public class LeaseDataCollection
{
    public IDictionary<string, LeaseData> Leases { get; set; } = new Dictionary<string, LeaseData>(StringComparer.OrdinalIgnoreCase);

    public bool Cleanup()
    {
        int count = Leases.Count;

        Leases = Leases
            .Where(x => x.Value.IsActive())
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        return count != Leases.Count;
    }

    public Option<LeaseData> Get(string leaseKey) => Leases.TryGetValue(leaseKey, out var leaseData) switch
    {
        false => StatusCode.NotFound,
        true => leaseData switch
        {
            var v when v.IsActive() => v,
            _ => StatusCode.NotFound,
        }
    };

    public bool Remove(string leaseKey) => Leases.Remove(leaseKey);

    public Option TryAdd(LeaseData leaseData)
    {
        if (Leases.TryGetValue(leaseData.LeaseKey, out var v) && !v.IsActive()) return (StatusCode.Conflict, "Lease key is already active");

        Leases[leaseData.LeaseKey] = leaseData;
        return StatusCode.OK;
    }
};
