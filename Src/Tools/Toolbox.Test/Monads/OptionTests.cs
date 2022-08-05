using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Monads;
using Xunit;

namespace Toolbox.Test.Monads;

public class OptionTests
{
    [Fact]
    public void TypeConversionTest_ShouldPass()
    {
        var str = "hello".Option();
        var length = str.Bind(x => x.Length);
        length.Return().Should().Be(5);
    }

    [Fact]
    public void ValidValues_WhenMatch_ShouldPass()
    {
        int? value = null;
        bool noValue = false;

        5.Option()
            .Bind(x => x + 10)
            .Bind(x => x + 20)
            .Match(x => value = x, () => noValue = true);

        value.Should().NotBeNull().And.Be(35);
        noValue.Should().Be(false);
    }

    [Fact]
    public void NotValidValues_WhenMatch_ShouldPass()
    {
        int? value = null;
        bool noValue = false;

        5.Option()
            .Bind(x => x + 10)
            .Bind(x => Option<int>.None)
            .Bind(x => x + 20)
            .Match(x => value = x, () => noValue = true);

        value.Should().BeNull();
        noValue.Should().Be(true);
    }

    [Fact]
    public void ValidValues_WhenMatchWithReturn_ShouldPass()
    {
        int result = 5.Option()
            .Bind(x => x + 10)
            .Bind(x => x + 20)
            .Switch(x => x, () => -1);

        result.Should().Be(35);
    }

    [Fact]
    public void NotValidValues_WhenMatchWithReturn_ShouldPass()
    {
        var result = 5.Option()
            .Bind(x => x + 10)
            .Bind(x => Option<int>.None)
            .Bind(x => x + 20)
            .Switch(x => x, () => -1);

        result.Should().Be(-1);
    }
}
