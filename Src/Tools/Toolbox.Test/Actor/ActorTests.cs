using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Actor.Host;
using Toolbox.Extensions;
using Xunit;

namespace Toolbox.Test.Actor
{
    [Trait("Category", "Actor")]
    public class ActorTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerFactory();

        [Fact]
        public async Task GivenActor_WhenCreated_KeyAndManagerShouldBeSet()
        {
            int count = 0;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => count += y));

            ActorKey key = new ActorKey("cache/test");
            ICache cache = actorHost.GetActor<ICache>(key);
            cache.GetActorKey().Should().Be(key);
            cache.GetActorManager().Should().Be(actorHost);

            count.Should().Be(1);
            (await actorHost.Deactivate<ICache>(key)).Should().BeTrue();
            count.Should().Be(0);

            await actorHost.DeactivateAll();
            count.Should().Be(0);
        }

        [Fact]
        public async Task GivenActor_WhenMultipleCreated_KeyAndManagerShouldBeSet()
        {
            int count = 0;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => count += y));

            const int max = 10;
            var keyList = new List<ActorKey>();

            Enumerable.Range(0, max)
                .ForEach((x, index) =>
                {
                    ActorKey key = new ActorKey($"cache/test_{index}");
                    keyList.Add(key);

                    ICache cache = actorHost.GetActor<ICache>(key);
                    cache.GetActorKey().Should().Be(key);
                    cache.GetActorManager().Should().Be(actorHost);
                });

            count.Should().Be(max);

            keyList
                .ForEach(x =>
                {
                    ICache cache = actorHost.GetActor<ICache>(x);
                    cache.GetActorKey().Should().Be(x);
                    cache.GetActorManager().Should().Be(actorHost);
                });

            count.Should().Be(max);

            await keyList
                .ForEachAsync(async x =>
                {
                    (await actorHost.Deactivate<ICache>(x)).Should().BeTrue();
                });

            count.Should().Be(0);

            await actorHost.DeactivateAll();
            count.Should().Be(0);
        }

        [Fact]
        public async Task GivenActor_WhenCreatedDeactivated_CountsShouldFollowLifecycle()
        {
            int count = 0;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => count += y));

            ActorKey key = new ActorKey("cache/test");
            ICache cache = actorHost.GetActor<ICache>(key);

            count.Should().Be(1);
            (await actorHost.Deactivate<ICache>(key)).Should().BeTrue();
            count.Should().Be(0);

            await actorHost.DeactivateAll();
            count.Should().Be(0);
        }

        [Fact]
        public async Task GivenActor_WhenDeactivatedAll_ActorCountShouldBeZero()
        {
            int count = 0;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => count += y));

            ActorKey key = new ActorKey("cache/test");
            ICache cache = actorHost.GetActor<ICache>(key);

            count.Should().Be(1);
            await actorHost.DeactivateAll();

            count.Should().Be(0);
        }

        [Fact]
        public async Task ActorMultipleTest()
        {
            int count = 0;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => count += y));

            ActorKey key1 = new ActorKey("Cache/Test1");
            ICache cache1 = actorHost.GetActor<ICache>(key1);
            count.Should().Be(1);

            ActorKey key2 = new ActorKey("Cache/Test2");
            ICache cache2 = actorHost.GetActor<ICache>(key2);
            count.Should().Be(2);

            (await actorHost.Deactivate<ICache>(key1)).Should().BeTrue();
            count.Should().Be(1);

            (await actorHost.Deactivate<ICache>(key2)).Should().BeTrue();
            count.Should().Be(0);

            await actorHost.DeactivateAll();
        }

        [Fact]
        public async Task ActorMethodTest()
        {
            int count = 0;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => count += y));

            ActorKey key1 = new ActorKey("Cache/Test1");
            ICache cache1 = actorHost.GetActor<ICache>(key1);
            count.Should().Be(1);

            const string firstText = "first";

            bool test = await cache1.IsCached(firstText);
            test.Should().BeFalse();
            await cache1.Add(firstText);
            test = await cache1.IsCached(firstText);
            test.Should().BeTrue();

            (await actorHost.Deactivate<ICache>(key1)).Should().BeTrue(); ;
            count.Should().Be(0);

            await actorHost.DeactivateAll();
        }

        [Fact]
        public async Task ActorSameInstanceTest()
        {
            int count = 0;

            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache(y => count += y));

            ActorKey key1 = new ActorKey("Cache/Test1");
            ICache cache1 = actorHost.GetActor<ICache>(key1);
            count.Should().Be(1);

            ActorKey key2 = new ActorKey("Cache/Test2");
            ICache cache2 = actorHost.GetActor<ICache>(key2);
            count.Should().Be(2);

            const string firstText = "first";
            const string secondText = "secondFirst";

            await cache1.Add(firstText);
            bool test = await cache1.IsCached(firstText);
            test.Should().BeTrue();

            await cache2.Add(secondText);
            bool test2 = await cache2.IsCached(secondText);
            test2.Should().BeTrue();

            ICache cache1Dup = actorHost.GetActor<ICache>(key1);
            test = await cache1Dup.IsCached(firstText);
            test.Should().BeTrue();
            test = await cache1Dup.IsCached(secondText);
            test.Should().BeFalse();

            (await actorHost.Deactivate<ICache>(key1)).Should().BeTrue();
            (await actorHost.Deactivate<ICache>(key2)).Should().BeTrue();
            count.Should().Be(0);

            await actorHost.DeactivateAll();
        }

        private interface ICache : IActor
        {
            Task<bool> IsCached(string key);

            Task Add(string key);

            ActorKey GetActorKey();

            IActorHost GetActorManager();
        }

        private class StringCache : ActorBase, ICache
        {
            private HashSet<string> _values = new HashSet<string>();

            public StringCache(Action<int> increment)
            {
                Increment = increment;
            }

            private Action<int> Increment { get; }

            public ActorKey GetActorKey() => ActorKey;

            public IActorHost GetActorManager() => ActorHost;

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

            public Task<bool> IsCached(string key)
            {
                return Task.FromResult(_values.Contains(key));
            }

            public Task Add(string key)
            {
                _values.Add(key);
                return Task.FromResult(0);
            }
        }
    }
}