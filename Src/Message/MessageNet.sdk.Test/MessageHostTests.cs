using FluentAssertions;
using MessageNet.sdk.Host;
using MessageNet.sdk.Protocol;
using MessageNet.sdk.Test.Application;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Tools;
using Xunit;

namespace MessageNet.sdk.Test
{
    [Collection("Real-Queue")]
    public class MessageHostTests
    {
        [Fact]
        public async Task GivenTwoNodes_WhenCalled_ReturnResponse()
        {
            const string sendText = "send message text";
            const string responseText = "response message text";

            ConcurrentQueue<MessagePacket> feMessages = new ConcurrentQueue<MessagePacket>();
            ConcurrentQueue<MessagePacket> accountMessages = new ConcurrentQueue<MessagePacket>();

            EndpointId feId = new EndpointId("main/fe");
            EndpointId accountId = new EndpointId("main/account");

            TestOption option = new TestOptionBuilder()
                .Build()
                .VerifyNotNull("Option failed");

            ILoggerFactory loggerFactory = LoggerFactory.Create(x => x.AddDebug().AddFilter(x => true));

            IMessageHost messageHost = new MessageHost(loggerFactory);

            messageHost.Register(option.Nodes.ToArray());

            messageHost.StartReceiver((string)feId, messagePacket =>
            {
                feMessages.Enqueue(messagePacket);
                return Task.CompletedTask;
            });

            messageHost.StartReceiver((string)accountId, async messagePacket =>
            {
                accountMessages.Enqueue(messagePacket);

                var content = new Content { ContentType = "responseType", Data = responseText };

                MessagePacket responsePacket = messagePacket.CreateResponseMessage(content);
                await messageHost.Send(responsePacket);
            });

            MessagePacket sendPacket = new MessagePacket
            {
                Messages = new[] { new Message
                {
                    ToEndpoint = accountId,
                    FromEndpoint = feId,
                    Contents = new []
                    {
                        new Content { ContentType = "contentType", Data = sendText }
                    }
                }},
            };

            MessagePacket resultPacket = await messageHost.Call(sendPacket);

            await Task.Delay(TimeSpan.FromSeconds(1));
            feMessages.Count.Should().Be(0);
            accountMessages.Count.Should().Be(1);

            resultPacket.Should().NotBeNull();
            resultPacket.Messages.Should().NotBeNull();
            resultPacket.Messages.Count.Should().Be(2);

            resultPacket.Messages.First().ToEndpoint.Should().Be(feId);
            resultPacket.Messages.First().FromEndpoint.Should().Be(accountId);
            resultPacket.Messages.First().Contents.Should().NotBeNull();
            resultPacket.Messages.First().Contents.First().ContentType.Should().Be("responseType");
            resultPacket.Messages.First().Contents.First().Data.Should().Be(responseText);

            resultPacket.Messages.Skip(1).First().ToEndpoint.Should().Be(accountId);
            resultPacket.Messages.Skip(1).First().FromEndpoint.Should().Be(feId);
            resultPacket.Messages.Skip(1).First().Contents.Should().NotBeNull();
            resultPacket.Messages.Skip(1).First().Contents.First().ContentType.Should().Be("contentType");
            resultPacket.Messages.Skip(1).First().Contents.First().Data.Should().Be(sendText);

            await ((MessageHost)messageHost).Stop();
        }
    }
}