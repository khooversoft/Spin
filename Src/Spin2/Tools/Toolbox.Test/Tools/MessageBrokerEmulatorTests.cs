using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class MessageBrokerEmulatorTests
{
    [Fact]
    public async Task SingleMessageMessageBrokerEmulator()
    {
        Queue<Message> queue = new Queue<Message>();
        ScopeContext context = new();
        var broker = new MessageBrokerEmulator(NullLogger<MessageBrokerEmulator>.Instance);

        broker.AddRoute<Message, int>("path", receiver);

        var msg = new Message
        {
            Data = "Data1",
        };

        int status = await broker.Send<Message, int>("path", msg, context);
        status.Should().Be(1);

        queue.Count.Should().Be(1);

        Task<int> receiver(Message msg, ScopeContext context)
        {
            queue.Enqueue(msg);
            return Task.FromResult(1);
        }
    }


    [Fact]
    public async Task MultipleMessageMessageBrokerEmulator()
    {
        const int count = 10;
        ScopeContext context = new();
        Queue<Message> queue = new Queue<Message>();
        var broker = new MessageBrokerEmulator(NullLogger<MessageBrokerEmulator>.Instance);

        broker.AddRoute<Message, bool>("path", receiver);

        foreach (var item in Enumerable.Range(0, count))
        {
            var msg = new Message
            {
                Data = $"Data1-{item}",
            };

            bool status = await broker.Send<Message, bool>("path", msg, context);
        }

        queue.Count.Should().Be(count);

        Task<bool> receiver(Message msg, ScopeContext context)
        {
            queue.Enqueue(msg);
            return Task.FromResult(true);
        }
    }

    private record Message
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Data { get; init; } = null!;
    }
}
