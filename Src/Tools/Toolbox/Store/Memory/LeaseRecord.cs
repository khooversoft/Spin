﻿using Toolbox.Tools;

namespace Toolbox.Store;

public record class LeaseRecord
{
    public LeaseRecord(string path, TimeSpan leaseDuration)
    {
        Path = path.NotNull();
        Duration = leaseDuration;

        (Infinite, Expiration) = (leaseDuration == TimeSpan.FromSeconds(-1)) switch
        {
            true => (true, DateTimeOffset.MaxValue),
            false => (false, DateTimeOffset.UtcNow + leaseDuration),
        };
    }

    public string Path { get; }
    public string LeaseId { get; } = Guid.NewGuid().ToString();
    public bool Infinite { get; }
    public TimeSpan Duration { get; }
    public DateTimeOffset Expiration { get; }

    public bool IsLeaseValid() => Infinite || Expiration > DateTimeOffset.UtcNow;
    public bool IsLeaseValid(string? leaseId) => IsLeaseValid() && (LeaseId != leaseId);
}
