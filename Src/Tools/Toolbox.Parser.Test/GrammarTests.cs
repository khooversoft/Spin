using FluentAssertions;
using System.Data;
using Toolbox.Extensions;
using Toolbox.Parser.Grammar;

namespace Toolbox.Parser.Test;

public class GrammarTests
{
    [Fact]
    public void GivenTypeDefinition_ShouldPass()
    {
        string line = "name = string";

        GrammarRule rule = RuleBuilder.Build(line.ToEnumerable());
        rule.Should().NotBeNull();
        rule.Rules.Count.Should().Be(1);

        (rule.Rules[0] is DataTypeRule dataTypeRule && dataTypeRule.Type == DataType.String).Should().BeTrue();
    }
}