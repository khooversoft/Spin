using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Actor.Host;
using Xunit;

namespace Toolbox.Test.Actor
{
    [Trait("Category", "Actor")]
    public class ActorChainCallTests
    {
        private const string sumActorName = "actorSum";
        private ILoggerFactory _loggerFactory = new TestLoggerFactory();

        [Fact]
        public async Task ActorSingleChainTest()
        {
            using IActorService actorHost = new ActorService(_loggerFactory)
                .Register<IActorNode>(() => new ActorNode())
                .Register<IActorSum>(() => new ActorSum());

            ActorKey key = new ActorKey("node/test");
            IActorNode node = actorHost.GetActor<IActorNode>(key);

            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                await node.Add(i);
                sum += i;
            }

            IActorSum sumActor = actorHost.GetActor<IActorSum>(new ActorKey(sumActorName));
            (await sumActor.GetSum()).Should().Be(sum);

            await actorHost.DeactivateAll();
        }

        private interface IActorNode : IActor
        {
            Task Add(int value);
        }

        private interface IActorSum : IActor
        {
            Task Add(int value);

            Task<int> GetSum();
        }

        private class ActorNode : ActorBase, IActorNode
        {
            private IActorSum? _actorSum;

            public ActorNode()
            {
            }

            public async Task Add(int value)
            {
                _actorSum = _actorSum ?? ActorHost.GetActor<IActorSum>(new ActorKey(sumActorName));
                await _actorSum.Add(value);
            }
        }

        private class ActorSum : ActorBase, IActorSum
        {
            private int _sum;

            public ActorSum()
            {
            }

            public Task Add(int value)
            {
                _sum += value;
                return Task.FromResult(0);
            }

            public Task<int> GetSum()
            {
                return Task.FromResult(_sum);
            }
        }
    }
}