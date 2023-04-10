using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Extensions;
using ToolBox.Azure.Test.Application;
using Xunit;

namespace ToolBox.Azure.Test.Queue;

public class QueueTests
{
    [Fact]
    public async Task RoundTripInSingleQueue_ShouldPass()
    {
        const string queueName = "round-trip-test";
        (QueueOption queueOption, QueueDefinition definition) = await ResetQueue(queueName);

        var client = new QueueClient<QueueMessage>(queueOption, TestHost.Default.CreateLogger<QueueClient<QueueMessage>>());

        var sendMessage = new Message
        {
            Name = "This is the name",
        };

        await client.Send(sendMessage.ToQueueMessage());

        TaskCompletionSource tcs = new TaskCompletionSource();
        QueueMessage? receivedQueueMessage = null;

        var receiverOption = new QueueReceiverOption<QueueMessage>
        {
            QueueOption = queueOption,
            Receiver = x =>
            {
                receivedQueueMessage = x;
                tcs.SetResult();
                return Task.FromResult(true);
            }
        };

        var receiver = new QueueReceiver<QueueMessage>(receiverOption, TestHost.Default.CreateLogger<QueueReceiver<QueueMessage>>());
        await receiver.Start();

        await Task.WhenAll(new[] { tcs.Task });
        receivedQueueMessage.Should().NotBeNull();

        await receiver.Stop();

        Message? receivedMessage = receivedQueueMessage!.GetContent<Message>();
        (sendMessage == receivedMessage).Should().BeTrue();

        await DeleteQueue(queueName);
    }

    [Fact]
    public async Task SendReceiveManyMessagesInSingleQueue_ShouldPass()
    {
        const int maxMessages = 10;
        const string queueName = "round-sendReceive-test";
        (QueueOption queueOption, QueueDefinition definition) = await ResetQueue(queueName);

        TaskCompletionSource sendTcs = new TaskCompletionSource();
        TaskCompletionSource receiveTcs = new TaskCompletionSource();

        var client = new QueueClient<QueueMessage>(queueOption, TestHost.Default.CreateLogger<QueueClient<QueueMessage>>());

        Task send = Task.Run(async () =>
        {
            await Enumerable.Range(0, maxMessages)
                .Select(x => new Message { Name = $"{x} Message" })
                .ForEachAsync(async x => await client.Send(x.ToQueueMessage()));

            sendTcs.SetResult();
        });

        var receiveQueue = new Queue<Message>();

        var receiverOption = new QueueReceiverOption<QueueMessage>
        {
            QueueOption = queueOption,
            Receiver = x =>
            {
                receiveQueue.Enqueue(x.GetContent<Message>());
                if( receiveQueue.Count == maxMessages) receiveTcs.SetResult();
                return Task.FromResult(true);
            }
        };

        var receiver = new QueueReceiver<QueueMessage>(receiverOption, TestHost.Default.CreateLogger<QueueReceiver<QueueMessage>>());
        await receiver.Start();

        await Task.WhenAll(new[] { send, sendTcs.Task, receiveTcs.Task });
        await receiver.Stop();

        receiveQueue.Count.Should().Be(maxMessages);
        var toMatch = Enumerable.Range(0, maxMessages)
            .Select(x => new Message { Name = $"{x} Message" });

        var result = receiveQueue
            .OrderBy(x => int.Parse(x.Name.Split(' ').First()))
            .Zip(toMatch);

        result.All(x => x.First == x.Second);

        await DeleteQueue(queueName);
    }

    private record Message
    {
        public string Name { get; set; } = null!;
    }

    private async Task<(QueueOption, QueueDefinition)> ResetQueue(string queueName)
    {
        QueueOption queueOption = TestHost.Default.GetQueueOption();
        queueOption = queueOption with { QueueName = queueName };

        QueueAdmin admin = new QueueAdmin(queueOption, TestHost.Default.CreateLogger<QueueAdmin>());

        bool exist = await admin.Exist(queueName);
        if (exist) await admin.Delete(queueName);

        var definition = new QueueDefinition
        {
            QueueName = queueName,
        };

        var createdDefinition = await admin.Create(definition);
        createdDefinition.Should().NotBeNull();

        return (queueOption, definition);
    }

    private async Task DeleteQueue(string queueName)
    {
        QueueOption queueOption = TestHost.Default.GetQueueOption();
        queueOption = queueOption with { QueueName = queueName };

        QueueAdmin admin = new QueueAdmin(queueOption, TestHost.Default.CreateLogger<QueueAdmin>());

        await admin.DeleteIfExist(queueName);
    }
}
