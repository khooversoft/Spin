//using FluentAssertions;
//using System.Data;
//using Toolbox.Extensions;
//using Toolbox.Parser.Grammar;

//namespace Toolbox.Parser.Test;

//public class GrammarTests
//{
//    [Fact]
//    public void GivenTypeOfStringDefinition_ShouldPass()
//    {
//        string line = "name = string";

//        GrammarRule rule = RuleBuilder.Build(line.ToEnumerable());
//        rule.Should().NotBeNull();
//        rule.Name.Should().Be("name");
//        rule.Rules.Count.Should().Be(1);

//        (rule.Rules[0] is DataTypeRule dataTypeRule && dataTypeRule.Type == DataType.String).Should().BeTrue();
//    }

//    [Fact]
//    public void GivenTypeOfIntDefinition_ShouldPass()
//    {
//        string line = "name = int";

//        GrammarRule rule = RuleBuilder.Build(line.ToEnumerable());
//        rule.Should().NotBeNull();
//        rule.Name.Should().Be("name");
//        rule.Rules.Count.Should().Be(1);

//        (rule.Rules[0] is DataTypeRule dataTypeRule && dataTypeRule.Type == DataType.Int).Should().BeTrue();
//    }

//    [Fact]
//    public void GivenChoiceDefinition_ShouldPass()
//    {
//        string line = "rank = 'prefix' | 'suffix'";

//        GrammarRule rule = RuleBuilder.Build(line.ToEnumerable());
//        rule.Should().NotBeNull();
//        rule.Name.Should().Be("name");
//        rule.Rules.Count.Should().Be(1);

//        (rule.Rules[0] is DataTypeRule dataTypeRule && dataTypeRule.Type == DataType.Int).Should().BeTrue();
//    }
//}