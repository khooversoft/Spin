using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Xunit;

namespace Toolbox.Test.Work;

public class AsyncEnumerableTests
{
    [Fact]
    public async Task WhenListOfAsyncFunctionsStandardPattern_ShouldPass()
    {
        var list = Enumerable.Range(0, 10).ToList();

        var sw = Stopwatch.StartNew();
        var sw2 = Stopwatch.StartNew();

        var services = list
            .Select(x => F(x))
            .ToList();

        await Task.WhenAll(services);

        sw.Stop();

        var results = services
            .Select(x => x.Result)
            .ToArray();

        sw2.Stop();

        TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds.Should().BeLessThan(10);
        TimeSpan.FromMilliseconds(sw2.ElapsedMilliseconds).TotalSeconds.Should().BeLessThan(10);
        results.Length.Should().Be(10);
    }

    [Fact]
    public async Task WhenListOfAsyncFunctions_ShouldPass()
    {
        var list = Enumerable.Range(0, 10).ToList();

        var sw = Stopwatch.StartNew();

        int[]? result = await list
            .Select(x => F(x))
            .FuncAsync(async x => await Task.WhenAll(x.ToArray()));

        sw.Stop();
        TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds.Should().BeLessThan(10);
        result.Length.Should().Be(10);
    }

    [Fact]
    public async Task WhenList_UsingWhenAll_ShouldPass()
    {
        var list = Enumerable.Range(0, 10).ToList();

        var sw = Stopwatch.StartNew();

        int[]? result = await list
            .Select(x => F(x))
            .WhenAll();

        sw.Stop();
        TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds.Should().BeLessThan(10);
        result.Length.Should().Be(10);
    }

    private async Task<int> F(int value)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        return value * 10;
    }
}
