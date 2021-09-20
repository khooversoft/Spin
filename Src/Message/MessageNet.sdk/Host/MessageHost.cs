using MessageNet.sdk.Models;
using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
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

namespace MessageNet.sdk.Host
{
    public class MessageHost : IMessageHost, IAsyncDisposable
    {
        private ConcurrentDictionary<string, MessageNodeOption> _registered = new ConcurrentDictionary<string, MessageNodeOption>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, QueueClient<MessagePacket>> _clients = new ConcurrentDictionary<string, QueueClient<MessagePacket>>(StringComparer.OrdinalIgnoreCase);
        private readonly AwaiterCollection<MessagePacket> _awaiterCollection;

        private MessageReceiverCollection<MessagePacket> _messageReceiverCollection;
        private readonly ILoggerFactory _loggerFactory;

        public MessageHost(ILoggerFactory loggerFactory)
        {
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _loggerFactory = loggerFactory;

            _awaiterCollection = new AwaiterCollection<MessagePacket>(_loggerFactory.CreateLogger<AwaiterCollection<MessagePacket>>());

            _messageReceiverCollection = new MessageReceiverCollectionBuilder()
                .SetAwaiterCollection(_awaiterCollection)
                .SetLoggerFactory(loggerFactory)
                .Build();
        }

        public MessageHost Register(params MessageNodeOption[] messageNodeOptions)
        {
            messageNodeOptions
                .ForEach(x => _registered.TryAdd(x.EndpointId, x).VerifyAssert(x => x == true, x => $"Endpoint already registered"));

            return this;
        }

        public QueueClient<MessagePacket> GetClient(string endpointId)
        {
            _registered.TryGetValue(endpointId, out MessageNodeOption? messageNodeOption)
                .VerifyAssert(x => x == true, x => $"Endpoint {x} is not registered");

            return _clients.GetOrAdd(endpointId, key => new MessageClientBuilder()
                {
                    LoggerFactory = _loggerFactory,
                    QueueOption = messageNodeOption!.BusQueue
                }.Build()
            );
        }

        public void StartReceiver(string endpointId, Func<MessagePacket, Task> receiver)
        {
            _registered.TryGetValue(endpointId, out MessageNodeOption? messageNodeOption)
                .VerifyAssert(x => x == true, x => $"Endpoint {x} is not registered");

            _messageReceiverCollection.Start(messageNodeOption!, receiver);
        }

        public async Task Send(MessagePacket messagePacket)
        {
            messagePacket.VerifyNotNull(nameof(messagePacket));
            string toEndpointId = (string)(messagePacket.GetMessage()?.ToEndpoint).VerifyNotNull($"{nameof(Message.ToEndpoint)} is required");

            await GetClient(toEndpointId).Send(messagePacket);
        }

        public async Task<MessagePacket> Call(MessagePacket messagePacket)
        {
            messagePacket.VerifyNotNull(nameof(messagePacket));
            Message message = messagePacket.GetMessage().VerifyNotNull("Message is required");
            string toEndpointId = ((string)message.ToEndpoint).VerifyNotEmpty($"{nameof(Message.ToEndpoint)} is required");

            await GetClient(toEndpointId).Send(messagePacket);

            var tcs = new TaskCompletionSource<MessagePacket>();
            _awaiterCollection.Register(message.MessageId, tcs);

            return await tcs.Task;
        }

        public async Task StopReceiver(string endpointId) => await _messageReceiverCollection.Stop((EndpointId)endpointId);

        public async Task Stop() => await _messageReceiverCollection.StopAll();

        public async ValueTask DisposeAsync() => await Stop();
    }
}
