using FluentAssertions;
using MessageNet.sdk.Host;
using MessageNet.sdk.Models;
using MessageNet.sdk.Protocol;
using MessageNet.Test.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;
using Xunit;

namespace MessageNet.Test
{
    public class MessageClientAndReceiverTests
    {
        [Fact]
        public async Task SingleMessage_WhenSent_ShouldRecord()
        {
            const string sendText = "send message text";

            ConcurrentQueue<MessagePacket> receivedMessages = new ConcurrentQueue<MessagePacket>();

            EndpointId queueId = new EndpointId("main/fe");

            TestOption option = new TestOptionBuilder()
                .Build()
                .VerifyNotNull("Option failed");

            ILoggerFactory loggerFactory = LoggerFactory.Create(x => x.AddDebug().AddFilter(x => true));

            MessageReceiverCollection<MessagePacket> messageHost = new MessageReceiverCollectionBuilder()
                .SetLoggerFactory(loggerFactory)
                .Build();

            MessageNodeOption nodeOption = option.Nodes.First(x => (EndpointId)x.EndpointId == queueId) with { AutoComplete = true };

            QueueClient<MessagePacket> queueClient = new MessageClientBuilder()
                .SetQueueOption(nodeOption.BusQueue)
                .SetLoggerFactory(loggerFactory)
                .Build();

            var tcs = new TaskCompletionSource();

            await messageHost.Start(nodeOption, x =>
            {
                receivedMessages.Enqueue(x);

                tcs.SetResult();
                return Task.CompletedTask;
            });

            MessagePacket packet = new MessagePacket
            {
                Messages = new[] { new Message
                {
                    FromEndpoint = queueId,
                    ToEndpoint = queueId,
                    Contents = new []
                    {
                        new Content { ContentType = "contentType", Data = sendText }
                    }
                }},
            };

            await queueClient.Send(packet);
            await tcs.Task;

            receivedMessages.Count.Should().Be(1);
            receivedMessages.TryDequeue(out MessagePacket? result).Should().BeTrue();

            (packet.Messages.Single().Contents.Single() == result!.Messages.Single().Contents.Single()).Should().BeTrue();

            await messageHost.StopAll();
        }

        [Fact]
        public async Task MultipleMessage_WhenSent_ShouldRecord()
        {
            const int max = 10;

            ConcurrentQueue<MessagePacket> receivedMessages = new ConcurrentQueue<MessagePacket>();

            EndpointId queueId = new EndpointId("main/account");

            TestOption option = new TestOptionBuilder()
                .Build()
                .VerifyNotNull("Option failed");

            ILoggerFactory loggerFactory = LoggerFactory.Create(x => x.AddDebug().AddFilter(x => true));

            MessageReceiverCollection<MessagePacket> messageHost = new MessageReceiverCollectionBuilder()
                .SetLoggerFactory(loggerFactory)
                .Build();

            MessageNodeOption nodeOption = option.Nodes.First(x => (EndpointId)x.EndpointId == queueId) with { AutoComplete = true };

            QueueClient<MessagePacket> queueClient = new MessageClientBuilder()
                .SetQueueOption(nodeOption.BusQueue)
                .SetLoggerFactory(loggerFactory)
                .Build();

            var tcs = new TaskCompletionSource();

            await messageHost.Start(nodeOption, x =>
            {
                receivedMessages.Enqueue(x);

                if (receivedMessages.Count == max) tcs.SetResult();
                return Task.CompletedTask;
            });

            IReadOnlyList<MessagePacket> packets = Enumerable.Range(0, max)
                .Select((x, i) => new MessagePacket
                {
                    Messages = new[] { new Message
                        {
                            FromEndpoint = queueId,
                            ToEndpoint = queueId,
                            Contents = new []
                            {
                                new Content { ContentType = "contentType", Data = $"Message {i}" }
                            }
                        },
                    }
                }).ToArray();

            await packets
                .ForEachAsync(x => queueClient.Send(x));

            await tcs.Task;

            receivedMessages.Count.Should().Be(max);

            await messageHost.StopAll();
        }
    }
}
