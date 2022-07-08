using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Actor.Host;
using Xunit;

namespace Toolbox.Test.Actor
{
    [Trait("Category", "Actor")]
    public class ActorProxyTests
    {
        private ILoggerFactory _loggerFactory = new TestLoggerFactory();

        [Fact]
        public async Task ActorProxyMultiTaskTest()
        {
            const int taskCount = 10;

            using IActorService actorHost = new ActorService(_loggerFactory)
                .Register<ICache>(() => new StringCache());

            var tasks = new List<Task>();
            ActorKey key1 = new ActorKey("Cache/Test1");
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            for (int i = 0; i < taskCount; i++)
            {
                Task t = Task.Run(() => TestAccess(actorHost, key1, tokenSource.Token));
                tasks.Add(t);
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
            tokenSource.Cancel();
            Task.WaitAll(tasks.ToArray());

            (await actorHost.Deactivate<ICache>(key1)).Should().BeTrue();
        }

        private async Task TestAccess(IActorService host, ActorKey actorKey, CancellationToken token)
        {
            const string firstText = "first";

            ICache cache1 = host.GetActor<ICache>(actorKey);
            while (!token.IsCancellationRequested)
            {
                bool test = await cache1.IsCached(firstText);
                await cache1.Add(firstText);
            }
        }

        public interface ICache : IActor
        {
            Task<bool> IsCached(string key);

            Task Add(string key);
        }

        public class StringCache : ActorBase, ICache
        {
            private HashSet<string> _values = new HashSet<string>();
            private int _lockValue = 0;

            public StringCache()
            {
            }

            public Task<bool> IsCached(string key)
            {
                int l = Interlocked.CompareExchange(ref _lockValue, 1, 0);
                if (l == 1)
                {
                    throw new InvalidOperationException("Locked accessed violation");
                }

                Interlocked.CompareExchange(ref _lockValue, 0, 1);
                return Task.FromResult(_values.Contains(key));
            }

            public Task Add(string key)
            {
                int l = Interlocked.CompareExchange(ref _lockValue, 1, 0);
                if (l == 1)
                {
                    throw new InvalidOperationException("Locked accessed violation");
                }

                Interlocked.CompareExchange(ref _lockValue, 0, 1);
                _values.Add(key);

                return Task.FromResult(0);
            }
        }
    }
}