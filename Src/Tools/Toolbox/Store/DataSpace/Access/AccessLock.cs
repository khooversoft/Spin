using Toolbox.Tools;

namespace Toolbox.Store;

public enum LockState
{
    Shared,
    Exclusive,
}

public record AccessLock
{
    public AccessLock(string path, string leaseId, LockState lockState, TimeSpan duration)
    {
        Path = path.NotEmpty();
        LeaseId = leaseId.NotEmpty();
        LockState = lockState;
        Duration = duration.Assert(x => x.TotalSeconds > 1, x => $"Invalid duration={x}");
    }

    public string Path { get; }
    public string LeaseId { get; }
    public LockState LockState { get; }
    public DateTime AcquiredDate { get; } = DateTime.UtcNow;
    public TimeSpan Duration { get; }
}
