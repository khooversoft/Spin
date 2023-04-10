//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Directory.sdk;
//using Directory.sdk.Model;
//using MessageNet.sdk.Protocol;
//using Microsoft.Extensions.Logging;
//using Toolbox.Azure.Queue;
//using Toolbox.Extensions;
//using Toolbox.Services;
//using Toolbox.Tools;

//namespace MessageNet.sdk.Host
//{
//    public class Receiver : IAsyncDisposable
//    {
//        private readonly IAwaiterCollection<Message> _awaiterCollection;
//        private readonly IDirectoryNameService _directoryNameService;
//        private readonly ILogger<Receiver> _logger;
//        private readonly Func<Message, Guid?> _getId;
//        private readonly IQueueReceiverFactory _queueReceiverFactory;
//        private readonly ConcurrentDictionary<string, IQueueReceiver> _queueReceivers = new ConcurrentDictionary<string, IQueueReceiver>(StringComparer.OrdinalIgnoreCase);

//        public Receiver(
//            Func<Message, Guid?> getId,
//            IQueueReceiverFactory queueReceiverFactory,
//            IAwaiterCollection<Message> awaiterCollection,
//            IDirectoryNameService directoryNameService,
//            ILogger<Receiver> logger
//            )
//        {
//            getId.VerifyNotNull(nameof(getId));
//            awaiterCollection.VerifyNotNull(nameof(awaiterCollection));
//            directoryNameService.VerifyNotNull(nameof(directoryNameService));
//            logger.VerifyNotNull(nameof(logger));

//            _getId = getId;
//            _queueReceiverFactory = queueReceiverFactory;
//            _awaiterCollection = awaiterCollection;
//            _directoryNameService = directoryNameService;
//            _logger = logger;
//        }

//        public async ValueTask DisposeAsync() => await StopAll();

//        public void Start(string serviceId, Func<Message, Task> receiver)
//        {
//            var db = _directoryNameService.Default;

//            QueueRecord queueRecord = db
//                .Service
//                .Get(serviceId, message: $"Cannot find service id {serviceId} in directory", logger: _logger)
//                .Func(x => db.Queue.Get(x.Channel, $"Cannot find channel{x.Channel} in directory", logger: _logger));

//            Start(serviceId, queueRecord, receiver);
//        }

//        public void Start(string serviceId, QueueRecord queueRecord, Func<Message, Task> receiver)
//        {
//            serviceId.VerifyNotEmpty(nameof(serviceId));
//            queueRecord.VerifyNotNull(nameof(queueRecord));
//            receiver.VerifyNotNull(nameof(receiver));

//            _logger.LogInformation($"Starting queue receiver {queueRecord}");

//            async Task interceptReceiver(Message message)
//            {
//                _logger.LogTrace($"{nameof(Start)}: Received message");

//                bool wasAwaiter = _getId(message) switch
//                {
//                    Guid id => _awaiterCollection.SetResult(id, message),

//                    _ => false,
//                };

//                switch (wasAwaiter)
//                {
//                    case true:
//                        _logger.LogTrace($"{nameof(Start)}: Called awaiter with message");
//                        break;

//                    default:
//                        _logger.LogTrace($"{nameof(Start)}: No awaiter, sending to receiver");
//                        await receiver(message);
//                        break;
//                }
//            }

//            var receiverOption = new QueueReceiverOption<Message>
//            {
//                QueueOption = queueRecord.ConvertTo(),
//                AutoComplete = queueRecord.AutoComplete,
//                MaxConcurrentCalls = queueRecord.MaxConcurrentCalls,

//                Receiver = interceptReceiver,
//            };

//            IQueueReceiver? queueReceiver = _queueReceiverFactory.Create<Message>(receiverOption);

//            _queueReceivers.TryAdd((string)serviceId, queueReceiver)
//                .VerifyAssert(x => x == true, $"Endpoint {serviceId} already registered");

//            _logger.LogInformation($"{nameof(Start)}: Starting queue receiver {serviceId}");
//            queueReceiver.Start();
//        }

//        public async Task<bool> Stop(string serviceId)
//        {
//            if (!_queueReceivers.TryRemove(serviceId, out IQueueReceiver? receiver)) return false;

//            await receiver.Stop();
//            return true;
//        }

//        public async Task StopAll()
//        {
//            var queue = new Queue<string>(_queueReceivers.Keys);

//            while (queue.TryDequeue(out string? key))
//            {
//                if (!_queueReceivers.TryRemove(key, out IQueueReceiver? queueReceiver)) continue;

//                _logger.LogInformation($"{nameof(Stop)}: Stopping queue receiver {key}, ");
//                await queueReceiver.Stop();
//            }
//        }
//    }
//}