namespace Toolbox.Azure.Queue
{
    public record QueueOption
    {
        public string Namespace { get; init; } = null!;

        public string QueueName { get; init; } = null!;

        public string KeyName { get; init; } = null!;

        public string AccessKey { get; init; } = null!;
    }
}
