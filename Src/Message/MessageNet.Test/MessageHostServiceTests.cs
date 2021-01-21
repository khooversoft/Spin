using FluentAssertions;
using MessageNet.sdk.Host;
using MessageNet.sdk.Protocol;
using MessageNet.Test.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;
using Xunit;

namespace MessageNet.Test
{
    public class MessageHostServiceTests
    {
        [Fact]
        public async Task UsingThreeNodesAndDi_WhenSentMessage_ShouldRoundTrip()
        {
            TestOption option = new TestOptionBuilder()
                .Build()
                .VerifyNotNull("Option failed");

            IServiceProvider service = new ServiceCollection()
                .AddLogging(x => x.AddDebug().AddFilter(x => true))
                .AddSingleton<IMessageHost, MessageHost>()
                .AddTransient<FeClient>()
                .AddTransient<ArtifactClient>()
                .AddTransient<AccountClient>()
                .BuildServiceProvider();

            IMessageHost messageHost = service.GetRequiredService<IMessageHost>();
            messageHost.Register(option.Nodes.ToArray());

            FeClient feClient = service.GetRequiredService<FeClient>();
            ArtifactClient artifactClient = service.GetRequiredService<ArtifactClient>();
            AccountClient accountClient = service.GetRequiredService<AccountClient>();

            MessagePacket messagePacket = await feClient.Call();

            await Task.Delay(TimeSpan.FromSeconds(5));

            messagePacket.Should().NotBeNull();

            messagePacket.Messages.Should().NotBeNull();
            messagePacket.Messages.Count.Should().Be(3);

            Verify(messagePacket.Messages.First(), (EndpointId)"main/fe", (EndpointId)"main/artifact", "artifactClient-type", "artifactClient-request");
            Verify(messagePacket.Messages.Skip(1).First(), (EndpointId)"main/account", (EndpointId)"main/artifact", "artifactClient-type", "artifactClient-request");
            Verify(messagePacket.Messages.Skip(2).First(), (EndpointId)"main/artifact", (EndpointId)"main/fe", "FeClient-type", "FeClient-request");
        }

        private void Verify(Message message, EndpointId toEndpointId, EndpointId fromEndpoint, string contentType, string data)
        {
            message.VerifyNotNull(nameof(message));

            message.ToEndpoint.Should().Be(toEndpointId);
            message.FromEndpoint.Should().Be(fromEndpoint);
            message.Contents.Should().NotBeNull();
            message.Contents.Count.Should().Be(1);
            message.Contents.First().ContentType.Should().Be(contentType);
            message.Contents.First().Data.Should().Be(data);
        }
    }

    internal class FeClient : ClientBase<FeClient>
    {
        public FeClient(IMessageHost messageHost, ILogger<FeClient> logger)
            : base(new EndpointId("main/fe"), new EndpointId("main/artifact"), messageHost, logger)
        {
        }
    }

    internal class ArtifactClient : ClientBase<FeClient>
    {
        private ActionBlock<MessagePacket> _sendAccount;

        public ArtifactClient(IMessageHost messageHost, ILogger<FeClient> logger)
            : base(new EndpointId("main/artifact"), new EndpointId("main/account"), messageHost, logger)
        {
            _sendAccount = new ActionBlock<MessagePacket>(async message =>
            {
                MessagePacket resultMessage = await _messageHost.Call(message);

                Message originalMessage = resultMessage.Messages.Last();

                var responseMessage = new Message
                {
                    FromEndpoint = (EndpointId)"main/artifact",
                    ToEndpoint = (EndpointId)"main/fe",
                    Contents = new[] { new Content { ContentType = "artifactClient-type", Data = "artifactClient-request" } },

                    OriginateMessageId = originalMessage.MessageId,
                };

                var responsePacket = message.WithMessage(responseMessage);

                await _messageHost.Send(responsePacket);
            });
        }

        public override Task Receiver(MessagePacket messagePacket)
        {
            var message = new Message
            {
                FromEndpoint = (EndpointId)"main/artifact",
                ToEndpoint = (EndpointId)"main/account",
                Contents = new[] { new Content { ContentType = "artifactClient-type", Data = "artifactClient-request" } }
            };

            var toAccount = messagePacket.WithMessage(message);
            _sendAccount.Post(toAccount);

            return Task.CompletedTask;
        }
    }

    internal class AccountClient : ClientBase<AccountClient>
    {
        public AccountClient(IMessageHost messageHost, ILogger<AccountClient> logger)
            : base(new EndpointId("main/account"), new EndpointId("main/fe"), messageHost, logger)
        {
        }
    }

    internal abstract class ClientBase<T> where T : class
    {
        protected readonly IMessageHost _messageHost;
        private readonly ILogger _logger;
        private readonly EndpointId _endpointId;
        private readonly EndpointId _sendToId;

        protected ClientBase(EndpointId endpointId, EndpointId sendToId, IMessageHost messageHost, ILogger logger)
        {
            _endpointId = endpointId;
            _sendToId = sendToId;
            _messageHost = messageHost;
            _logger = logger;

            messageHost.StartReceiver((string)_endpointId, Receiver);
        }

        public virtual async Task Receiver(MessagePacket messagePacket)
        {
            var content = new Content { ContentType = typeof(T).Name + "-type", Data = typeof(T).Name + "-response" };

            MessagePacket response = messagePacket.CreateResponseMessage(content);
            await _messageHost.Send(response);
        }

        public async Task<MessagePacket> Call()
        {
            var packet = new MessagePacket
            {
                Messages = new[]
                {
                    new Message
                    {
                        FromEndpoint = _endpointId,
                        ToEndpoint = _sendToId,
                        Contents = new [] { new Content { ContentType = typeof(T).Name + "-type", Data = typeof(T).Name + "-request" } }
                    }
                }
            };

            return await _messageHost.Call(packet);
        }
    }
}
