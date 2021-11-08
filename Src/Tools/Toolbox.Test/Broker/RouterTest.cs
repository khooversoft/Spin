using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Broker;
using Toolbox.Extensions;
using Xunit;

namespace Toolbox.Test.Broker
{
    public class RouterTest
    {
        [Fact]
        public async Task GivenRouteMessageReceived()
        {
            var sync = new TaskCompletionSource<int>();

            await new Router(new NullLogger<Router>())
                .Add(new Route<int>("*", x => { sync.SetResult(x); return Task.CompletedTask; }))
                .Send("*", 5);

            int receive = await sync.Task;
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

            var router = new Router(new NullLogger<Router>())
                .Action(x => list.ForEach(y => x.Add(new Route<int>(y.path, z => { y.tcs.SetResult(z); return Task.CompletedTask; }))));

            IReadOnlyList<string> randomList = list
                .Select(x => x.path)
                .Shuffle();

            int index = 0;

            await randomList.ForEachAsync(async x => await router.Send(x, index++));

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

            var router = new Router(new NullLogger<Router>())
                .Add(list.Select(x => new Route<int>(x.path, z => { x.queue.Enqueue(z); return Task.CompletedTask; })).ToArray())
                .SetForward(x => { failed.Enqueue((int)x); return Task.CompletedTask; });

            int index = 0;
            int loopCount = 9999;

            foreach (var loop in Enumerable.Range(0, loopCount))
            {
                await list
                    .ForEachAsync(async x => await router.Send(x.path, index++));
            }

            failed.Count.Should().Be(0);

            list
                .Count(x => x.queue.Count == loopCount)
                .Should().Be(list.Length);
        }
    }
}
