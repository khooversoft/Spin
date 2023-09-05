using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionTestTests
{
    [Fact]
    public void SandardFlow()
    {
        var optionTest = new OptionTest();

        optionTest.Test(() => StatusCode.OK);
        optionTest.IsOk().Should().BeTrue();
        optionTest.Option.StatusCode.Should().Be(StatusCode.OK);

        optionTest.Test(() => StatusCode.OK);
        optionTest.IsOk().Should().BeTrue();
        optionTest.Option.StatusCode.Should().Be(StatusCode.OK);

        optionTest.Test(() => StatusCode.BadRequest);
        optionTest.IsError().Should().BeTrue();
        optionTest.Option.StatusCode.Should().Be(StatusCode.BadRequest);

        optionTest.Test(() => StatusCode.OK);
        optionTest.IsError().Should().BeTrue();
        optionTest.Option.StatusCode.Should().Be(StatusCode.BadRequest);
    }

    [Fact]
    public void FluentFlowPositive()
    {
        var test = new OptionTest()
            .Test(() => StatusCode.OK)
            .Test(() => StatusCode.OK);

        test.IsOk().Should().BeTrue();
        test.IsError().Should().BeFalse();
    }

    [Fact]
    public void FluentFlowFaile()
    {
        var test = new OptionTest()
            .Test(() => StatusCode.OK)
            .Test(() => StatusCode.BadRequest)
            .Test(() => StatusCode.OK);

        test.IsOk().Should().BeFalse();
        test.IsError().Should().BeTrue();
    }
}
