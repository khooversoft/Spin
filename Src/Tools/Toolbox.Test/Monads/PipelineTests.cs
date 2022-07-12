using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Toolbox.Monads;

namespace Toolbox.Test.Monads;

public class PipelineTests
{
    [Fact]
    public void PipelineSimpleMathTest()
    {
        int result = 5
            .Maybe()
            .Bind<int>(x => x + 10)
            .Bind(x => (Maybe<int>)(x + 20))
            .Return();

        result.Should().Be(35);
    }

    [Fact]
    public void PipelineNullableMath_WhenStopped_ShouldPass()
    {
        int? result = 5
            .Maybe()
            .Bind<int?>(x => x + 10)
            .Bind<int?>(x => Maybe<int?>.None)
            .Bind(x => (Maybe<int?>)(x + 20))
            .Return();

        (result == Maybe<int?>.None).Should().BeTrue();
        (result == null).Should().BeTrue();
    }

    [Fact]
    public void PipelineMath_WhenStopped_ShouldPass()
    {
        int result = 5
            .Maybe()
            .Bind<int>(x => x + 10)
            .Bind<int>(x => Maybe<int>.None)
            .Bind(x => (Maybe<int>)(x + 20))
            .Return();

        (result == Maybe<int>.None).Should().BeTrue();
        (result == 0).Should().BeTrue();
    }

    [Fact]
    public void PipelineStringAppendTest()
    {
        string? result = "This is a test"
            .Maybe()
            .Bind<string>(x => x += "***")
            .Bind(x => new Maybe<string>(x.Replace(" ", "-")))
            .Return();

        result.Should().Be("This-is-a-test***");
    }

    [Fact]
    public void ConversionTest()
    {
        var none1 = new Maybe<int?>();
        var none = Maybe<int>.None;

        (none == Maybe<int>.None).Should().BeTrue();
        (none == new Maybe<int>()).Should().BeTrue();

        Maybe<int> result = 5;
        (result == 5).Should().BeTrue();

        int v = result;
        v.Should().Be(5);
    }
}
