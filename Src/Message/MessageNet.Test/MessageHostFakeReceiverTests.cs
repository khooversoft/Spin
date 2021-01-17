using System;
using Xunit;
using Toolbox.Azure.Queue;
using System.Threading.Tasks;
using Toolbox.Tools;
using System.Threading;
using FluentAssertions;
using MessageNet.sdk.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using MessageNet.sdk.Models;
using MessageNet.sdk.Protocol;
using Toolbox.Services;
using Toolbox.Extensions;
using System.Linq;

namespace MessageNet.Test
{
    public class MessageHostFakeReceiverTests
    {
        [Fact]
        public async Task GivenFakeReceiver_WhenSingleMessageSent_ShouldReceivelMessage()
        {
            Func<FakeMessage, Guid?> getId = x => x.Id;
            ConcurrentQueue<FakeReceiver<FakeMessage>> receivers = new ConcurrentQueue<FakeReceiver<FakeMessage>>();
            ConcurrentQueue<FakeMessage> fakeMessages = new ConcurrentQueue<FakeMessage>();
            IAwaiterCollection<FakeMessage> awaiterCollection = new AwaiterCollection<FakeMessage>();

            IQueueReceiverFactory queueReceiverFactory = new FakeReceiverFactory(x => receivers.Enqueue((FakeReceiver<FakeMessage>)x));

            MessageHost<FakeMessage> messageHost = new MessageHost<FakeMessage>(getId, queueReceiverFactory, awaiterCollection, new NullLogger<MessageHost<FakeMessage>>());

            MessageNodeOption option = GetMessageNodeOption();

            await messageHost.Start(option, x =>
            {
                fakeMessages.Enqueue(x);
                return Task.CompletedTask;
            });

            var message = new FakeMessage
            {
                Message = "this is a message",
            };

            var tcs = new TaskCompletionSource<FakeMessage>();
            awaiterCollection.Add(message.Id, tcs);

            receivers.Count.Should().Be(1);
            receivers.TryDequeue(out FakeReceiver<FakeMessage>? fakeReceiver).Should().BeTrue();

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                fakeReceiver!.Send(message);
            });

            Task.WaitAll(new[] { tcs.Task }, TimeSpan.FromSeconds(50)).Should().BeTrue();

            FakeMessage receivedMessage = tcs.Task.Result;
            (message == receivedMessage).Should().BeTrue();

            fakeMessages.TryDequeue(out FakeMessage? receivedMessage2).Should().BeTrue();
            (message == receivedMessage2).Should().BeTrue();

            (await messageHost.Stop((EndpointId)option.EndpointId)).Should().BeTrue();
        }

        [Fact]
        public async Task GivenFakeReceiver_WhenMultipleMessageSent_ShouldReceivelMessage()
        {
            const int max = 10;
            Random rnd = new Random();
            Func<FakeMessage, Guid?> getId = x => x.Id;
            ConcurrentQueue<FakeReceiver<FakeMessage>> receivers = new ConcurrentQueue<FakeReceiver<FakeMessage>>();
            ConcurrentQueue<FakeMessage> fakeMessages = new ConcurrentQueue<FakeMessage>();
            IAwaiterCollection<FakeMessage> awaiterCollection = new AwaiterCollection<FakeMessage>();

            IQueueReceiverFactory queueReceiverFactory = new FakeReceiverFactory(x => receivers.Enqueue((FakeReceiver<FakeMessage>)x));

            MessageHost<FakeMessage> messageHost = new MessageHost<FakeMessage>(getId, queueReceiverFactory, awaiterCollection, new NullLogger<MessageHost<FakeMessage>>());

            MessageNodeOption option = GetMessageNodeOption();

            await messageHost.Start(option, x =>
            {
                fakeMessages.Enqueue(x);
                return Task.CompletedTask;
            });

            var messages = Enumerable.Range(0, max)
                .Select(x => new FakeMessage { Message = $"this is {x} message" })
                .Select(x => (FakeMessage: x, Tcs: new TaskCompletionSource<FakeMessage>()))
                .ToArray();

            messages
                .ForEach(x => awaiterCollection.Add(x.FakeMessage.Id, x.Tcs));

            receivers.Count.Should().Be(1);
            receivers.TryDequeue(out FakeReceiver<FakeMessage>? fakeReceiver).Should().BeTrue();

            messages
                .ForEach(x => _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(1, 300)));
                    fakeReceiver!.Send(x.FakeMessage);
                }));

            Task.WaitAll(messages.Select(x => x.Tcs.Task).ToArray(), TimeSpan.FromSeconds(50)).Should().BeTrue();

            fakeMessages.Count.Should().Be(max);

            var results = messages
                .Select(x => x.Tcs.Task.Result)
                .ToArray();

            results.Length.Should().Be(max);

            var testResults = messages.OrderBy(x => x.FakeMessage.Message)
                .Zip(results.OrderBy(x => x.Message), (message, result) => (message, result))
                .ToArray();

            testResults
                .All(x => x.message.FakeMessage.Message == x.result.Message)
                .Should().BeTrue();

            (await messageHost.Stop((EndpointId)option.EndpointId)).Should().BeTrue();
        }

        private class FakeReceiver<T> : IQueueReceiver where T : class
        {
            private int _state;
            private const int _stopped = 0;
            private const int _started = 1;
            private readonly Func<T, Task> _receiver;

            public FakeReceiver(Func<T, Task> receiver)
            {
                _receiver = receiver;
            }

            public Task Start()
            {
                Interlocked.CompareExchange(ref _state, _started, _stopped).Should().Be(_stopped);
                return Task.CompletedTask;
            }

            public Task Stop()
            {
                Interlocked.CompareExchange(ref _state, _stopped, _started).Should().Be(_started);
                return Task.CompletedTask;
            }

            public void Send(T message) => _receiver(message);
        }

        private class FakeReceiverFactory : IQueueReceiverFactory
        {
            private readonly Action<IQueueReceiver> _onCreate;

            public FakeReceiverFactory(Action<IQueueReceiver> onCreate)
            {
                _onCreate = onCreate;
            }

            public IQueueReceiver Create<T>(QueueOption queueOption, Func<T, Task> receiver) where T : class
            {
                return new FakeReceiver<T>(receiver)
                    .Action(x => _onCreate(x));
            }
        }

        private class FakeMessage
        {
            public Guid Id { get; } = Guid.NewGuid();

            public string Message { get; set; } = null!;
        }

        private static MessageNodeOption GetMessageNodeOption() => new MessageNodeOption
        {
            EndpointId = "ns/node",
            BusQueue = new QueueOption
            {
                Namespace = "ns",
                QueueName = "queueName",
                KeyName = "keyName",
                AccessKey = "accessKey"
            }
        };
    }
}
