using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class RouterTest
    {
        [Fact]
        public async Task GivenRouteMessageReceived()
        {
            var sync = new TaskCompletionSource<int>();

            int v = new Router<int, int>(new NullLogger<Router<int, int>>())
               .Add("*", (x, _) => { sync.SetResult(x); return 0; })
               .Send("*", 5, CancellationToken.None);

            v.Should().Be(0);

            int receive = await sync.Task;
            receive.Should().Be(5);
        }
        
        [Fact]
        public void GivenSimpleRouteMessage_ShouldReceive()
        {
            int v = new Router<int, int>(new NullLogger<Router<int, int>>())
               .Add("*", (x, c) => x * 10)
               .Send("*", 5, CancellationToken.None);

            v.Should().Be(50);
        }   
        
        [Fact]
        public async Task GivenSimpleAsyncRouteMessage_ShouldReceive()
        {
            int v = await new Router<int, Task<int>>(new NullLogger<Router<int, Task<int>>>())
               .Add("*", (x, c) => Task.FromResult(x * 10))
               .Send("*", 5, CancellationToken.None);

            v.Should().Be(50);
        }

        [Fact]
        public async Task GivenMultipleRoutesAllMessagesAreReceived()
        {
            var list = new (TaskCompletionSource<int> tcs, string path)[]
            {
                (new TaskCompletionSource<int>(),  "1"),
                (new TaskCompletionSource<int>(),  "2"),
                (new TaskCompletionSource<int>(),  "3"),
                (new TaskCompletionSource<int>(),  "4"),
                (new TaskCompletionSource<int>(),  "5"),
                (new TaskCompletionSource<int>(),  "6"),
            };

            var router = new Router<int, int>(new NullLogger<Router<int, int>>())
                .Action(x => list.ForEach(y => x.Add(y.path, (z, _) => { y.tcs.SetResult(z); return 0; })));

            IReadOnlyList<string> randomList = list
                .Select(x => x.path)
                .Shuffle();

            int index = 0;

            randomList.ForEach(x => router.Send(x, index++, CancellationToken.None));

            int[] results = await Task.WhenAll(list.Select(x => x.tcs.Task).ToArray());

            results.Length.Should().Be(list.Length);

            results.OrderBy(x => x)
                .Zip(Enumerable.Range(0, list.Length), (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();
        }

        [Fact]
        public async Task GivenMultipleRoutesAllMessagesAreReceivedOvertime()
        {
            var list = new (ConcurrentQueue<int> queue, string path)[]
            {
                (new ConcurrentQueue<int>(),  "1"),
                (new ConcurrentQueue<int>(),  "2"),
                (new ConcurrentQueue<int>(),  "3"),
                (new ConcurrentQueue<int>(),  "4"),
                (new ConcurrentQueue<int>(),  "5"),
                (new ConcurrentQueue<int>(),  "6"),
            };

            var failed = new ConcurrentQueue<int>();

            var router = new Router<int, Task>(new NullLogger<Router<int, Task>>())
                .Action(x => list.ForEach(y => x.Add(y.path, (z, _) => { y.queue.Enqueue(z); return Task.CompletedTask; })))
                .Add("*", (x, _) => { failed.Enqueue(x); return Task.CompletedTask; });

            int index = 0;
            int loopCount = 9999;

            foreach (var loop in Enumerable.Range(0, loopCount))
            {
                await list
                    .ForEachAsync(async x => await router.Send(x.path, index++, CancellationToken.None));
            }

            await router.Send("*", -1, CancellationToken.None);

            failed.Count.Should().Be(1);

            list
                .Count(x => x.queue.Count == loopCount)
                .Should().Be(list.Length);
        }
    }
}
