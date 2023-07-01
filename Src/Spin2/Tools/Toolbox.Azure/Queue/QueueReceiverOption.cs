namespace Toolbox.Azure.Queue
{
    public record QueueReceiverOption<T> where T : class
    {
        public QueueOption QueueOption { get; init; } = null!;

        public Func<T, Task<bool>> Receiver { get; init; } = null!;

        public bool AutoComplete { get; init; } = false;

        public int MaxConcurrentCalls { get; init; } = 10;
    }
}
