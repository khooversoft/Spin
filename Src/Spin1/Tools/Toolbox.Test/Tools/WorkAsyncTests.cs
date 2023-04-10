using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class WorkAsyncTests
    {
        [Fact]
        public async Task GivenWork_ShouldPass()
        {
            var work = new WorkAsync<int, int>(x => x, 1);

            Enumerable.Range(0, 10).ForEach(x => work.Post(x));

            IReadOnlyList<int> result = await work.Complete();
            result.Should().NotBeNull();
            result.Count.Should().Be(10);

            Enumerable.Range(0, 10)
                .Zip(result, (o, i) => (o, i))
                .All(x => x.o == x.i)
                .Should().BeTrue();
        }

        [Fact]
        public async Task GiveWork_When_Parallel()
        {
            var work = new WorkAsync<int, (int, int)>(x =>
            {
                return (Thread.CurrentThread.ManagedThreadId, x);
            }, 2);

            Enumerable.Range(0, 10).ForEach(x => work.Post(x));

            IReadOnlyList<(int ThreadId, int Index)> result = await work.Complete();
            result.Should().NotBeNull();
            result.Count.Should().Be(10);

            Enumerable.Range(0, 10)
                .Zip(result.OrderBy(x => x.Index), (o, i) => (o, i))
                .All(x => x.o == x.i.Index)
                .Should().BeTrue();
        }
    }
}
