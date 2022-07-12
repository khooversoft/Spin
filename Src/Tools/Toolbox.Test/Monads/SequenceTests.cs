using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Monads;
using Xunit;

namespace Toolbox.Test.Monads;

public class SequenceTests
{
    [Fact]
    public void GetFirstAnswer_ShouldPass()
    {
        Dictionary<string, string> answer = new Dictionary<string, string>()
        {
            ["first"] = "first answer",
            ["second"] = "second answer",
            ["*"] = "any answer",
        };

        var result = "second".Seq()
            .Bind(x => answer.TryGetValue(x))
            .Bind(x => answer.TryGetValue("*"))
            .Return()
            .First(x => x.HasValue)
            .Return();

        result.Should().Be("second answer");
    }
    
    [Fact]
    public void GetDefaultAnswer_ShouldPass()
    {
        Dictionary<string, string> answer = new Dictionary<string, string>()
        {
            ["first"] = "first answer",
            ["second"] = "second answer",
            ["*"] = "any answer",
        };

        var result = "no found".Seq()
            .Bind(x => answer.TryGetValue(x))
            .Bind(x => answer.TryGetValue("*"))
            .Return()
            .First(x => x.HasValue)
            .Return();

        result.Should().Be("any answer");
    }
}
