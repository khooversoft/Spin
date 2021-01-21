using MessageNet.sdk.Models;
using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Services;
using Toolbox.Tools;

namespace MessageNet.sdk.Host
{
    public class MessageReceiverCollection : MessageReceiverCollection<MessagePacket>
    {
        public MessageReceiverCollection(
            Func<MessagePacket, Guid?> getId,
            IQueueReceiverFactory queueReceiverFactory,
            IAwaiterCollection<MessagePacket> awaiterCollection,
            ILogger<MessageReceiverCollection<MessagePacket>> logger
            )
            : base(getId, queueReceiverFactory, awaiterCollection, logger)
        {
        }
    }

    public class MessageReceiverCollection<T> : IAsyncDisposable where T : class
    {
        private readonly IAwaiterCollection<T> _awaiterCollection;
        private readonly ILogger<MessageReceiverCollection<T>> _logger;
        private readonly Func<T, Guid?> _getId;
        private readonly IQueueReceiverFactory _queueReceiverFactory;
        private readonly ConcurrentDictionary<string, IQueueReceiver> _queueReceivers = new ConcurrentDictionary<string, IQueueReceiver>(StringComparer.OrdinalIgnoreCase);

        public MessageReceiverCollection(Func<T, Guid?> getId, IQueueReceiverFactory queueReceiverFactory, IAwaiterCollection<T> awaiterCollection, ILogger<MessageReceiverCollection<T>> logger)
        {
            getId.VerifyNotNull(nameof(getId));
            awaiterCollection.VerifyNotNull(nameof(awaiterCollection));
            logger.VerifyNotNull(nameof(logger));

            _getId = getId;
            _queueReceiverFactory = queueReceiverFactory;
            _awaiterCollection = awaiterCollection;
            _logger = logger;
        }

        public async ValueTask DisposeAsync() => await StopAll();

        public void Start(MessageNodeOption messageNodeOption, Func<T, Task> receiver)
        {
            messageNodeOption.VerifyNotNull(nameof(messageNodeOption));
            receiver.VerifyNotNull(nameof(receiver));

            Func<T, Task> interceptReceiver = async message =>
            {
                bool wasAwaiter = _getId(message) switch
                {
                    Guid id => _awaiterCollection.SetResult(id, message),

                    _ => false,
                };

                if (!wasAwaiter)
                {
                    await receiver(message);
                }
            };

            var receiverOption = new QueueReceiverOption<T>
            {
                QueueOption = messageNodeOption.BusQueue,
                AutoComplete = messageNodeOption.AutoComplete,
                MaxConcurrentCalls = messageNodeOption.MaxConcurrentCalls,

                Receiver = interceptReceiver,
            };

            IQueueReceiver? queueReceiver = _queueReceiverFactory.Create<T>(receiverOption);

            _queueReceivers.TryAdd((string)messageNodeOption.EndpointId, queueReceiver)
                .VerifyAssert(x => x == true, $"Endpoint {messageNodeOption} already registered");

            _logger.LogInformation($"{nameof(Start)}: Starting queue receiver {messageNodeOption.EndpointId}, ");
            queueReceiver.Start();
        }

        public async Task<bool> Stop(EndpointId endpointId)
        {
            if (!_queueReceivers.TryRemove((string)endpointId, out IQueueReceiver? receiver)) return false;

            await receiver.Stop();
            return true;
        }

        public async Task StopAll()
        {
            var queue = new Queue<string>(_queueReceivers.Keys);

            while (queue.TryDequeue(out string? key))
            {
                if (!_queueReceivers.TryRemove(key, out IQueueReceiver? queueReceiver)) continue;

                _logger.LogInformation($"{nameof(Stop)}: Stopping queue receiver {key}, ");
                await queueReceiver.Stop();
            }
        }
    }
}