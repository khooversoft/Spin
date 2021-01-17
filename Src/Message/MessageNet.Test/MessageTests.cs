using FluentAssertions;
using MessageNet.sdk.Host;
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
    public class MessageTests
    {
        [Fact]
        public async Task SingleMessage_WhenSent_ShouldRecord()
        {
            const string sendText = "send message text";
            const string responseText = "response message text";

            ConcurrentQueue<MessagePacket> receivedMessages = new ConcurrentQueue<MessagePacket>();

            EndpointId fromId = new EndpointId("main/fe");
            EndpointId toId = new EndpointId("main/account");

            TestOption option = new TestOptionBuilder()
                .Build()
                .VerifyNotNull("Option failed");

            ILoggerFactory loggerFactory = LoggerFactory.Create(x => x.AddDebug());

            MessageHost<MessagePacket> messageHost = new MessageHostBuilder()
                .SetLoggerFactory(loggerFactory)
                .Build();

            QueueClient<MessagePacket> fromClient = new MessageClientBuilder()
                .SetQueueOption(option.Nodes.First(x => (EndpointId)x.EndpointId == fromId).BusQueue)
                .SetLoggerFactory(loggerFactory)
                .Build();

            QueueClient<MessagePacket> toClient = new MessageClientBuilder()
                .SetQueueOption(option.Nodes.First(x => (EndpointId)x.EndpointId == toId).BusQueue)
                .SetLoggerFactory(loggerFactory)
                .Build();

            await messageHost.Start(option.Nodes.First(x => (EndpointId)x.EndpointId == toId), async x =>
            {
                receivedMessages.Enqueue(x);

                var responseContent = new Content
                {
                    ContentType = "response",
                    Data = responseText.ToBytes(),
                };

                MessagePacket returnPacket = x.CreateResponseMessage(responseContent);
                await toClient.Send(returnPacket);
            });

            MessagePacket packet = new MessagePacket
            {
                Messages = new[] { new Message
                {
                    FromEndpoint = fromId,
                    ToEndpoint = toId,
                    Contents = new []
                    {
                        new Content { ContentType = "contentType", Data = sendText.ToBytes() }
                    }
                }},
            };

            MessagePacket receivePacket = await toClient.Call(packet);

            receivedMessages.Count.Should().Be(1);
            receivePacket.GetMessage()
                .VerifyNotNull("no message")
                .Contents.First().Data
                .VerifyNotNull("no data")
                .ToString().Should().Be(responseText);

            await messageHost.StopAll();
        }
    }
}
