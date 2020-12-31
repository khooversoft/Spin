using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor.Host;
using Toolbox.Actor.Test.Application;
using Xunit;
using Xunit.Abstractions;
using Toolbox.Extensions;
using Toolbox.Actor;

namespace Toolbox.Test.Actor
{
    public class MultipleActorTests
    {
        private readonly ITestOutputHelper _output;
        private ILoggerFactory _loggerFactory = new TestLoggerFactory();

        public MultipleActorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Given2Actors_WhenCreatedAndDeleted_ShouldPass()
        {
            int count = 0;
            int count2 = 0;
            const int max = 10;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => CountControl(ref count, y)))
                .Register<ICache2>(() => new StringCache2(y => CountControl(ref count2, y)));

            Enumerable.Range(0, max)
                .ForEach(x =>
                {
                    ActorKey key = new ActorKey($"cache/test/{x}");
                    ICache cache = actorHost.GetActor<ICache>(key);
                    cache.GetActorKey().Should().Be(key);
                    cache.GetActorHost().Should().Be(actorHost);
                    cache.Test(key);

                    ActorKey key2 = new ActorKey($"cache/test/{max-x}");
                    ICache2 cache2 = actorHost.GetActor<ICache2>(key2);
                    cache2.GetActorKey().Should().Be(key2);
                    cache2.GetActorHost().Should().Be(actorHost);
                    cache2.Test(key2);
                });

            count.Should().Be(max);
            count2.Should().Be(max);

            Enumerable.Range(0, max)
                .ForEach(x =>
                {
                    ActorKey key = new ActorKey($"cache/test/{x}");
                    ICache cache = actorHost.GetActor<ICache>(key);
                    cache.GetActorKey().Should().Be(key);
                    cache.GetActorKey().Key.Should().Be(key.Key);
                    cache.GetActorKey().Value.Should().Be(key.Value);
                    cache.GetActorHost().Should().Be(actorHost);
                    cache.Test(key);

                    ActorKey key2 = new ActorKey($"cache/test/{max-x}");
                    ICache2 cache2 = actorHost.GetActor<ICache2>(key2);
                    cache2.GetActorKey().Should().Be(key2);
                    cache2.GetActorKey().Key.Should().Be(key2.Key);
                    cache2.GetActorKey().Value.Should().Be(key2.Value);
                    cache2.GetActorHost().Should().Be(actorHost);
                    cache2.Test(key2);
                });

            await Enumerable.Range(0, max)
                .Select(async x =>
                {
                    ActorKey key = new ActorKey($"cache/test/{x}");
                    (await actorHost.Deactivate<ICache>(key)).Should().BeTrue();

                    ActorKey key2 = new ActorKey($"cache/test/{max-x}");
                    (await actorHost.Deactivate<ICache2>(key2)).Should().BeTrue();
                })
                .WhenAll();

            count.Should().Be(0);
            count2.Should().Be(0);

            await actorHost.DeactivateAll();
            count.Should().Be(0);
            count2.Should().Be(0);
        }

        [Fact]
        public async Task Given2Actors_WhenCreatedAndDeletedDifferentTask_ShouldPass()
        {
            int count = 0;
            int count2 = 0;
            const int max = 10;
            const int maxLoop = 10;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => CountControl(ref count, y)))
                .Register<ICache2>(() => new StringCache2(y => CountControl(ref count2, y)));

            for (int loop = 0; loop < maxLoop; loop++)
            {
                _output.WriteLine($"Loop: {loop}");

                await Enumerable.Range(0, max)
                    .Select(x => new Task[]
                    {
                        Task.Run(() => {
                            ActorKey key = new ActorKey($"cache/test/{x}");
                            ICache cache = actorHost.GetActor<ICache>(key);
                            cache.GetActorKey().Should().Be(key);
                            cache.GetActorHost().Should().Be(actorHost);
                        }),
                        Task.Run(() => {
                            ActorKey key2 = new ActorKey($"cache/test/{x}");
                            ICache2 cache2 = actorHost.GetActor<ICache2>(key2);
                            cache2.GetActorKey().Should().Be(key2);
                            cache2.GetActorHost().Should().Be(actorHost);
                        }),
                    })
                    .SelectMany(x => x)
                    .WhenAll();

                count.Should().Be(max);
                count2.Should().Be(max);

                await Enumerable.Range(0, max)
                    .Select(x => new Task<bool>[]
                    {
                        Task.Run(async () => await actorHost.Deactivate<ICache>(new ActorKey($"cache/test/{x}"))),
                        Task.Run(async () => await actorHost.Deactivate<ICache2>(new ActorKey($"cache/test/{x}"))),
                    })
                    .SelectMany(x => x)
                    .WhenAll();

                count.Should().Be(0);
                count2.Should().Be(0);

                await actorHost.DeactivateAll();
                count.Should().Be(0);
                count2.Should().Be(0);
            }
        }

        [Fact]
        public async Task Given2Actors_WhenCreatedAndDeletedDifferentKeyRange_ShouldPass()
        {
            int count = 0;
            int count2 = 0;
            const int max = 1000;
            const int maxLoop = 10;

            using IActorHost actorHost = new ActorHost(100000, _loggerFactory)
                .Register<ICache>(() => new StringCache(y => CountControl(ref count, y)))
                .Register<ICache2>(() => new StringCache2(y => CountControl(ref count2, y)));

            for (int loop = 0; loop < maxLoop; loop++)
            {
                _output.WriteLine($"Loop: {loop}");

                await Enumerable.Range(0, max)
                    .Select((x, i) => new Task[]
                    {
                        Task.Run(() => actorHost.GetActor<ICache>(new ActorKey($"cache/test/{i}"))),
                        Task.Run(() => actorHost.GetActor<ICache2>(new ActorKey($"cache/test/{i+100}"))),
                    })
                    .SelectMany(x => x)
                    .WhenAll();

                count.Should().Be(max);
                count2.Should().Be(max);

                var results = await Enumerable.Range(0, max)
                    .Select((x, i) => new Task<bool>[]
                    {
                        Task.Run(async () => await actorHost.Deactivate<ICache>(new ActorKey($"cache/test/{i}"))),
                        Task.Run(async () => await actorHost.Deactivate<ICache2>(new ActorKey($"cache/test/{i+100}"))),
                    })
                    .SelectMany(x => x)
                    .WhenAll();

                results.All(x => x == true).Should().BeTrue();

                count.Should().Be(0);
                count2.Should().Be(0);

                await actorHost.DeactivateAll();
                count.Should().Be(0);
                count2.Should().Be(0);
            }
        }

        private void CountControl(ref int subject, int value)
        {
            if (value > 0)
            {
                Interlocked.Increment(ref subject);
            }
            else
            {
                Interlocked.Decrement(ref subject);
            }
        }

        private interface ICache : IActor
        {
            Task<bool> IsCached(string key);
            Task Add(string key);
            Task Test(ActorKey key);
            ActorKey GetActorKey();
            IActorHost GetActorHost();
        }

        private class StringCache : ActorBase, ICache
        {
            private HashSet<string> _values = new HashSet<string>();

            public StringCache(Action<int> increment) => Increment = increment;

            private Action<int> Increment { get; }
            public ActorKey GetActorKey() => ActorKey;
            public IActorHost GetActorHost() => ActorHost;

            protected override Task OnActivate()
            {
                Increment(1);
                return base.OnActivate();
            }

            protected override Task OnDeactivate()
            {
                Increment(-1);
                return base.OnDeactivate();
            }

            public Task<bool> IsCached(string key) => Task.FromResult(_values.Contains(key));

            public Task Add(string key)
            {
                _values.Add(key);
                return Task.FromResult(0);
            }

            public Task Test(ActorKey key)
            {
                base.ActorKey.Should().Be(key, this.GetType().Name);
                return Task.CompletedTask;
            }
        }

        private interface ICache2 : IActor
        {
            Task<bool> IsCached(string key);
            Task Add(string key);
            Task Test(ActorKey key);
            ActorKey GetActorKey();
            IActorHost GetActorHost();
        }

        private class StringCache2 : ActorBase, ICache2
        {
            private HashSet<string> _values = new HashSet<string>();

            public StringCache2(Action<int> increment) => Increment = increment;

            private Action<int> Increment { get; }
            public ActorKey GetActorKey() => ActorKey;
            public IActorHost GetActorHost() => ActorHost;

            protected override Task OnActivate()
            {
                Increment(1);
                return base.OnActivate();
            }

            protected override Task OnDeactivate()
            {
                Increment(-1);
                return base.OnDeactivate();
            }

            public Task<bool> IsCached(string key) => Task.FromResult(_values.Contains(key));

            public Task Add(string key)
            {
                _values.Add(key);
                return Task.FromResult(0);
            }

            public Task Test(ActorKey key)
            {
                base.ActorKey.Should().Be(key, this.GetType().Name);
                return Task.CompletedTask;
            }
        }
    }
}
