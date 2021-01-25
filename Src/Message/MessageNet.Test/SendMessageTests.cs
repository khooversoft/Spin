using FluentAssertions;
using MessageNet.sdk.Client;
using MessageNet.sdk.Protocol;
using MessageNet.Test.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MessageNet.Test
{
    public class SendMessageTests
    {
        [Fact]
        public async Task RegisterAndRemove_ShouldSucces()
        {
            TestWebsiteHost host = TestApplication.GetHost();
            RegisterClient registerClient = host.GetRegisterClient();

            EndpointId toEndpoint = new EndpointId("main/account");

            (await registerClient.Register(toEndpoint, new Uri("http://localhost:5010/api/sync"))).Should().BeTrue();

            (await registerClient.Remove(toEndpoint)).Should().BeTrue();
        }

        [Fact]
        public async Task SendMessage_ShouldPass()
        {
            TestWebsiteHost host = TestApplication.GetHost();
            RegisterClient registerClient = host.GetRegisterClient();
            MessageClient messageClient = host.GetMessageClient();

            EndpointId toEndpoint = new EndpointId("main/account");
            EndpointId fromEndpoint = new EndpointId("main/fe");
            TestCallbackReceiver testCallbackReceiver = host.Resolve<TestCallbackReceiver>();

            // Register first
            (await registerClient.Register(toEndpoint, new Uri("http://localhost:5010/api/sync"))).Should().BeTrue();

            const string contentType = "contentType";
            const string data = "this is the data";

            // Send message
            var messagePacket = new MessagePacket
            {
                Messages = new[]
                {
                    new Message
                    {
                        FromEndpoint = fromEndpoint,
                        ToEndpoint = toEndpoint,
                        Contents = new [] { new Content { ContentType = contentType, Data = data } }
                    }
                }
            };

            await messageClient.Send(messagePacket);

            await Task.Delay(TimeSpan.FromSeconds(2));

            IReadOnlyList<MessagePacket> queue = testCallbackReceiver.GetMessages();
            queue.Should().NotBeNull();
            queue.Count.Should().Be(1);

            MessagePacket receivedPacket = queue.First();

            receivedPacket.Messages.Count.Should().Be(1);
            receivedPacket.Messages.First().ToEndpoint.Should().Be(toEndpoint);
            receivedPacket.Messages.First().FromEndpoint.Should().Be(fromEndpoint);
            receivedPacket.Messages.First().Contents.Count.Should().Be(1);
            receivedPacket.Messages.First().Contents.First().ContentType.Should().Be(contentType);
            receivedPacket.Messages.First().Contents.First().Data.Should().Be(data);

            await registerClient.Remove(toEndpoint);
        }
    }
}
