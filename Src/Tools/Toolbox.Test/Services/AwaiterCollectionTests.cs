using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Services;
using Xunit;

namespace Toolbox.Test.Services
{
    public class AwaiterCollectionTests
    {
        [Fact]
        public void SingleAsync_WhenSignaled_ShouldComplete()
        {
            var awaiter = new AwaiterCollection<bool>();

            Guid id = Guid.NewGuid();
            var tcs = new TaskCompletionSource<bool>();

            awaiter.Register(id, tcs);

            CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

            _ = Task.Run(async () =>
            {
                while (tokenSource.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                }

                awaiter.SetResult(id, true);
            });

            Task.WaitAll(new Task[] { tcs.Task }, TimeSpan.FromSeconds(30)).Should().BeTrue();

            tcs.Task.Result.Should().BeTrue();
        }

        [Fact]
        public void MultipleAsync_WhenSingle_ShouldComplete()
        {
            const int max = 100;
            var random = new Random();
            var awaiter = new AwaiterCollection<int>();

            IReadOnlyList<(Guid Id, int Index, int DelayMs)> details = Enumerable.Range(0, max)
                .Select((x, i) => (Id: Guid.NewGuid(), Index: i, DelayMs: random.Next(50, 300)))
                .ToArray();

            IReadOnlyList<(Guid Id, TaskCompletionSource<int> Tcs, CancellationTokenSource TokenSource, int Index)> list = details
                .Select(x => (Id: x.Id, Tcs: new TaskCompletionSource<int>(), TokenSource: new CancellationTokenSource(x.DelayMs), Index: x.Index))
                .ToArray()
                .Action(x => x.ForEach(y => awaiter.Register(y.Id, y.Tcs)));

            list.ForEach(x => Task.Run(async () =>
            {
                var xSave = x;

                while(!xSave.TokenSource.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                }

                awaiter.SetResult(xSave.Id, xSave.Index);
            }));

            Task.WaitAll(list.Select(x => x.Tcs.Task).ToArray(), TimeSpan.FromSeconds(30)).Should().BeTrue();

            list.All(x => x.Tcs.Task.Result == x.Index).Should().BeTrue();
        }
    }
}
