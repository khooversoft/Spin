//using FluentAssertions;
//using MessageNet.sdk.Host;
//using MessageNet.sdk.Models;
//using MessageNet.sdk.Protocol;
//using Microsoft.Extensions.Logging.Abstractions;
//using System;
//using System.Collections.Concurrent;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Toolbox.Azure.Queue;
//using Toolbox.Extensions;
//using Toolbox.Services;
//using Xunit;

//namespace MessageNet.sdk.Test
//{
//    public class MessageFakeReceiverTests
//    {
//        [Fact]
//        public async Task GivenFakeReceiver_WhenSingleMessageSent_ShouldReceivelMessage()
//        {
//            Func<FakeMessage, Guid?> getId = x => x.Id;
//            ConcurrentQueue<FakeReceiver<FakeMessage>> receivers = new ConcurrentQueue<FakeReceiver<FakeMessage>>();
//            IAwaiterCollection<FakeMessage> awaiterCollection = new AwaiterCollection<FakeMessage>(new NullLogger<AwaiterCollection<FakeMessage>>());

//            IQueueReceiverFactory queueReceiverFactory = new FakeReceiverFactory(x => receivers.Enqueue((FakeReceiver<FakeMessage>)x));

//            MessageReceiverCollection<FakeMessage> messageReceiverCollection = new MessageReceiverCollection<FakeMessage>(getId, queueReceiverFactory, awaiterCollection, new NullLogger<MessageReceiverCollection<FakeMessage>>());

//            MessageNodeOption option = GetMessageNodeOption();

//            messageReceiverCollection.Start(option, x =>
//            {
//                throw new InvalidOperationException("Receiver should not be called");
//            });

//            var message = new FakeMessage
//            {
//                Message = "this is a message",
//            };

//            var tcs = new TaskCompletionSource<FakeMessage>();
//            awaiterCollection.Register(message.Id, tcs);

//            receivers.Count.Should().Be(1);
//            receivers.TryDequeue(out FakeReceiver<FakeMessage>? fakeReceiver).Should().BeTrue();

//            _ = Task.Run(async () =>
//            {
//                await Task.Delay(TimeSpan.FromMilliseconds(100));
//                fakeReceiver!.Send(message);
//            });

//            Task.WaitAll(new[] { tcs.Task }, TimeSpan.FromSeconds(50)).Should().BeTrue();

//            FakeMessage receivedMessage = tcs.Task.Result;
//            (message == receivedMessage).Should().BeTrue();

//            (await messageReceiverCollection.Stop((EndpointId)option.EndpointId)).Should().BeTrue();
//        }

//        [Fact]
//        public async Task GivenFakeReceiver_WhenMultipleMessageSent_ShouldReceivelMessage()
//        {
//            const int max = 10;
//            Random rnd = new Random();
//            Func<FakeMessage, Guid?> getId = x => x.Id;
//            ConcurrentQueue<FakeReceiver<FakeMessage>> receivers = new ConcurrentQueue<FakeReceiver<FakeMessage>>();
//            IAwaiterCollection<FakeMessage> awaiterCollection = new AwaiterCollection<FakeMessage>(new NullLogger<AwaiterCollection<FakeMessage>>());

//            IQueueReceiverFactory queueReceiverFactory = new FakeReceiverFactory(x => receivers.Enqueue((FakeReceiver<FakeMessage>)x));

//            MessageReceiverCollection<FakeMessage> messageReceiverCollection = new MessageReceiverCollection<FakeMessage>(getId, queueReceiverFactory, awaiterCollection, new NullLogger<MessageReceiverCollection<FakeMessage>>());

//            MessageNodeOption option = GetMessageNodeOption();

//            messageReceiverCollection.Start(option, x =>
//            {
//                throw new InvalidOperationException("Receiver should not be called");
//            });

//            var messages = Enumerable.Range(0, max)
//                .Select(x => new FakeMessage { Message = $"this is {x} message" })
//                .Select(x => (FakeMessage: x, Tcs: new TaskCompletionSource<FakeMessage>()))
//                .ToArray();

//            messages
//                .ForEach(x => awaiterCollection.Register(x.FakeMessage.Id, x.Tcs));

//            receivers.Count.Should().Be(1);
//            receivers.TryDequeue(out FakeReceiver<FakeMessage>? fakeReceiver).Should().BeTrue();

//            messages
//                .ForEach(x => _ = Task.Run(async () =>
//                {
//                    await Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(1, 300)));
//                    fakeReceiver!.Send(x.FakeMessage);
//                }));

//            Task.WaitAll(messages.Select(x => x.Tcs.Task).ToArray(), TimeSpan.FromSeconds(50)).Should().BeTrue();

//            var results = messages
//                .Select(x => x.Tcs.Task.Result)
//                .ToArray();

//            results.Length.Should().Be(max);

//            var testResults = messages.OrderBy(x => x.FakeMessage.Message)
//                .Zip(results.OrderBy(x => x.Message), (message, result) => (message, result))
//                .ToArray();

//            testResults
//                .All(x => x.message.FakeMessage.Message == x.result.Message)
//                .Should().BeTrue();

//            (await messageReceiverCollection.Stop((EndpointId)option.EndpointId)).Should().BeTrue();
//        }

//        private class FakeReceiver<T> : IQueueReceiver where T : class
//        {
//            private int _state;
//            private const int _stopped = 0;
//            private const int _started = 1;
//            private readonly Func<T, Task> _receiver;

//            public FakeReceiver(Func<T, Task> receiver)
//            {
//                _receiver = receiver;
//            }

//            public void Start()
//            {
//                Interlocked.CompareExchange(ref _state, _started, _stopped).Should().Be(_stopped);
//            }

//            public Task Stop()
//            {
//                Interlocked.CompareExchange(ref _state, _stopped, _started).Should().Be(_started);
//                return Task.CompletedTask;
//            }

//            public void Send(T message) => _receiver(message);
//        }

//        private class FakeReceiverFactory : IQueueReceiverFactory
//        {
//            private readonly Action<IQueueReceiver> _onCreate;

//            public FakeReceiverFactory(Action<IQueueReceiver> onCreate)
//            {
//                _onCreate = onCreate;
//            }

//            public IQueueReceiver Create<T>(QueueReceiverOption<T> queueReceiver) where T : class
//            {
//                return new FakeReceiver<T>(queueReceiver.Receiver)
//                    .Action(x => _onCreate(x));
//            }
//        }

//        private class FakeMessage
//        {
//            public Guid Id { get; } = Guid.NewGuid();

//            public string Message { get; set; } = null!;
//        }

//        private static MessageNodeOption GetMessageNodeOption() => new MessageNodeOption
//        {
//            EndpointId = "ns/node",
//            BusQueue = new QueueOption
//            {
//                Namespace = "ns",
//                QueueName = "queueName",
//                KeyName = "keyName",
//                AccessKey = "accessKey"
//            }
//        };
//    }
//}