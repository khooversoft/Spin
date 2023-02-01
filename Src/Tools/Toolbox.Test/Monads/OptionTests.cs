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
    public void TestNone()
    {
        (Option<int>.None == Option<int>.None).Should().BeTrue();
        ((false, default(int)).Option() == Option<int>.None).Should().BeTrue();
        (5.Option() != Option<int>.None).Should().BeTrue();
        (Option<int>.None != 5.Option()).Should().BeTrue();

        (5.Option() == 5.Option()).Should().BeTrue();

        (new Option<string>("value") == new Option<string>("value")).Should().BeTrue();
        (new Option<string>("value") == "value".Option()).Should().BeTrue();
        (new Option<int?>(null) == new Option<int?>()).Should().BeTrue();
        (new Option<int?>(null) != new Option<int?>(5)).Should().BeTrue();

        (new Option<int>(10).Equals("hello")).Should().BeFalse();
        (new Option<string>("hello").Equals("hello")).Should().BeTrue();
    }

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
        var value = 5.Option()
            .Bind(x => x + 10)
            .Bind(x => x + 20)
            .Return();

        value.Should().Be(35);

        var value1 = 10.Option()
            .Bind(x => x + 10)
            .Bind(x => x + 20) switch
        {
            var v when v == Option<int>.None => -1,
            var v => v.Return(),
        };

        value1.Should().Be(40);
    }

    [Fact]
    public void NotValidValues_WhenMatch_ShouldPass()
    {
        var value = 5.Option()
            .Bind(x => x + 10)
            .Bind(x => Option<int>.None)
            .Bind(x => x + 20) switch
        {
            var v when !v.HasValue => -1,
            var v => v.Return(),
        };

        value.Should().Be(-1);

        var value1 = 5.Option()
            .Bind(x => x + 10)
            .Bind(x => Option<int>.None)
            .Bind(x => x + 20) switch
        {
            var v when v == Option<int>.None => -1,
            var v => v.Return(),
        };

        value.Should().Be(-1);
    }
}
