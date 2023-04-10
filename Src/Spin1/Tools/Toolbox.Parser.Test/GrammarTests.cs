using FluentAssertions;
using System.Data;
using Toolbox.Extensions;
using Toolbox.Parser.Grammar;
using Toolbox.Parser.Syntax;
using Toolbox.Tools;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Test;

public class GrammarTests
{
    [Fact]
    public void GivenPropertyTypeDefinition_ShouldPass()
    {
        var grammarTree = new Tree()
            + (new RuleNode("propertyType") + new LiteralRule() + new TokenRule("=") + new LiteralRule() + new TokenRule(";"));

        string line = "name = string;";

        Tree? syntaxTree = new SyntaxTreeBuilder().Build(line, grammarTree);
        syntaxTree.Should().NotBeNull();
        var ruleNode = syntaxTree!.OfType<RuleNodeValue>().FirstOrDefault();
        ruleNode.Should().NotBeNull();
        ruleNode!.Name.Should().Be("propertyType");
        ruleNode.Count.Should().Be(4);

        ruleNode.OfType<LiteralRuleValue>().FirstOrDefault().NotNull().Value.Should().Be("name");
        ruleNode.OfType<TokenRuleValue>().FirstOrDefault().NotNull().Value.Should().Be("=");
        ruleNode.OfType<LiteralRuleValue>().Skip(1).FirstOrDefault().NotNull().Value.Should().Be("string");
        ruleNode.OfType<TokenRuleValue>().Skip(1).FirstOrDefault().NotNull().Value.Should().Be(";");
    }

    [Fact]
    public void GivenVariableDefinition_ShouldPass()
    {
        var grammarTree = new Tree()
            + (new RuleNode("variable") + new LiteralRule() + new TokenRule("=") + new LiteralRule(LiteralType.String) + new TokenRule(";"));

        string line = "name = \"this is a text string\";";

        Tree? syntaxTree = new SyntaxTreeBuilder().Build(line, grammarTree);
        syntaxTree.Should().NotBeNull();

        var ruleNode = syntaxTree!.OfType<RuleNodeValue>().FirstOrDefault();
        ruleNode.Should().NotBeNull();
        ruleNode!.Name.Should().Be("variable");
        ruleNode.Count.Should().Be(4);

        ruleNode.OfType<LiteralRuleValue>().FirstOrDefault().NotNull().Value.Should().Be("name");
        ruleNode.OfType<TokenRuleValue>().FirstOrDefault().NotNull().Value.Should().Be("=");
        ruleNode.OfType<LiteralRuleValue>().Skip(1).FirstOrDefault().NotNull().Value.Should().Be("this is a text string");
        ruleNode.OfType<TokenRuleValue>().Skip(1).FirstOrDefault().NotNull().Value.Should().Be(";");
    }

    [Fact]
    public void GivenVariableNotStringDefinition_ShouldPass()
    {
        var grammarTree = new Tree()
            + (new RuleNode("variable") + new LiteralRule() + new TokenRule("=") + new LiteralRule() + new TokenRule(";"));

        string line = "name = \"this is a text string\";";

        Tree? syntaxTree = new SyntaxTreeBuilder().Build(line, grammarTree);
        syntaxTree.Should().NotNull();
    }
}