using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types.Maybe;

namespace Toolbox.Test.Types;

public class OptionTests
{
    [Fact]
    public void StatusOptionTest()
    {
        var o1 = new Option();
        (o1 == default).Should().BeTrue();
        (o1.StatusCode == OptionStatus.NoContent).Should().BeTrue();

        Option o2 = OptionStatus.Created.ToOption();
        o2.StatusCode.Should().Be(OptionStatus.Created);
        (o1 != default).Should().BeTrue();

        Option o3 = new Option(OptionStatus.Created);
        o3.StatusCode.Should().Be(OptionStatus.Created);
        (o2 == o3).Should().BeTrue();

        OptionStatus o4 = o3;
        (o4 == o3).Should().BeTrue();

        Option o5 = o4;
        (o5 == OptionStatus.Created).Should().BeTrue();
    }
}
