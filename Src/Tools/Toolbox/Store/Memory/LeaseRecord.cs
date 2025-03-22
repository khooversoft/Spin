using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Store;

internal record class LeaseRecord
{
    public LeaseRecord(string path, TimeSpan leaseDuration)
    {
        Path = path.NotNull();
        leaseDuration.Assert(x => x.TotalSeconds > 30 && x.TotalMinutes < 5, "Lease duration must be between 30 seconds and 5 minutes");

        (Infinite, Expiration) = (leaseDuration == TimeSpan.FromSeconds(-1)) switch
        {
            true => (true, DateTimeOffset.MaxValue),
            false => (false, DateTimeOffset.UtcNow + leaseDuration),
        };
    }

    public string Path { get; }
    public string LeaseId { get; } = Guid.NewGuid().ToString();
    public bool Infinite { get; }
    public DateTimeOffset Expiration { get; }

    public bool IsLeaseValid() => Expiration > DateTimeOffset.UtcNow;
    public bool IsLeaseValid(string? leaseId) => IsLeaseValid() && (LeaseId != leaseId);
}
