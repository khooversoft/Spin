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
using Toolbox.Tools;

namespace MessageNet.sdk.Host
{
    public class MessageHost : IAsyncDisposable
    {
        private ConcurrentDictionary<string, MessageNodeOption> _registered = new ConcurrentDictionary<string, MessageNodeOption>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, QueueClient<MessagePacket>> _clients = new ConcurrentDictionary<string, QueueClient<MessagePacket>>(StringComparer.OrdinalIgnoreCase);

        private MessageReceiverCollection<MessagePacket> _messageReceiverCollection;
        private readonly ILoggerFactory _loggerFactory;

        public MessageHost(ILoggerFactory loggerFactory)
        {
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _loggerFactory = loggerFactory;

            _messageReceiverCollection = new MessageReceiverCollectionBuilder()
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
                .SetLoggerFactory(_loggerFactory)
                .SetQueueOption(messageNodeOption!.BusQueue)
                .Build()
                );
        }

        public async Task StartReceiver(string endpointId, Func<MessagePacket, Task> receiver)
        {
            _registered.TryGetValue(endpointId, out MessageNodeOption? messageNodeOption)
                .VerifyAssert(x => x == true, x => $"Endpoint {x} is not registered");

            await _messageReceiverCollection.Start(messageNodeOption!, receiver);
        }

        public async Task StopReceiver(string endpointId) => await _messageReceiverCollection.Stop((EndpointId)endpointId);

        public async Task Stop() => await _messageReceiverCollection.StopAll();

        public async ValueTask DisposeAsync() => await Stop();
    }
}
