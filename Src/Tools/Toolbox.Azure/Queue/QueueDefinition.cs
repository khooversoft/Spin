using System;

namespace Toolbox.Azure.Queue;

public record QueueDefinition
{
    public string QueueName { get; init; } = null!;

    public TimeSpan LockDuration { get; init; } = TimeSpan.FromSeconds(45);

    public bool RequiresDuplicateDetection { get; init; }

    public TimeSpan DuplicateDetectionHistoryTimeWindow { get; init; } = TimeSpan.FromMinutes(10);

    public bool RequiresSession { get; init; }

    public TimeSpan DefaultMessageTimeToLive { get; init; } = TimeSpan.FromMinutes(30);

    public TimeSpan AutoDeleteOnIdle { get; init; } = TimeSpan.FromDays(30);

    public bool EnableDeadLetteringOnMessageExpiration { get; init; }

    public int MaxDeliveryCount { get; init; } = 8;

    public bool EnablePartitioning { get; init; }
}
