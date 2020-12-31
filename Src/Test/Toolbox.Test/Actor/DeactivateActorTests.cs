using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Actor.Host;
using Toolbox.Actor.Test.Application;
using Xunit;

namespace Toolbox.Test.Actor
{
    [Trait("Category", "Actor")]
    public class DeactivateActorTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerFactory();

        [Fact]
        public async Task GivenActor_WhenDeactivated_ShouldPass()
        {
            using IActorHost actorHost = new ActorHost(_loggerFactory)
                .Register<ICache>(() => new StringCache());

            ActorKey actorKey = new ActorKey("Cache/Test1");
            ICache cache = actorHost.GetActor<ICache>(actorKey);

            int count = await cache.GetCount();
            count.Should().Be(1);

            await cache.TestAndDeactivate();
            count = await cache.GetCount();
            count.Should().Be(2);

            await cache.TestAndDeactivate();
            count = await cache.GetCount();
            count.Should().Be(4);
        }

        private interface ICache : IActor
        {
            Task<int> GetCount();

            Task TestAndDeactivate();
        }

        private class StringCache : ActorBase, ICache
        {
            private int _count = 0;

            public StringCache()
            {
            }

            protected override Task OnActivate()
            {
                _count++;
                return base.OnActivate();
            }

            protected override Task OnDeactivate()
            {
                _count++;
                return base.OnDeactivate();
            }

            public Task<int> GetCount() => Task.FromResult(_count);

            public Task TestAndDeactivate()
            {
                if (_count++ >= 2) return Deactivate();

                return Task.CompletedTask;
            }
        }
    }
}
