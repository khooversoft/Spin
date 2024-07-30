using FluentAssertions;
using Toolbox.LangTools;

namespace Toolbox.Test.LangTools.Meta;

public class ModelEqualTests
{
    [Fact]
    public void CompareTerminal()
    {
        var p1 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        var p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"], Index = 1 };
        (p1 == p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();

        // Name
        p2 = new TerminalSymbol { Name = null!, Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        // Name
        p2 = new TerminalSymbol { Name = "numberx", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        // Text
        p2 = new TerminalSymbol { Name = "number", Text = "x[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        // Terminal type
        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Token, Tags = ["t1"] };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        // Tags
        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t2"] };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1", "t2"] };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();
    }

    [Fact]
    public void CompareProductionRuleReference()
    {
        var p1 = new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol" };
        var p2 = new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol", Index = 1 };
        (p1 == p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();

        p2 = new ProductionRuleReference { Name = null!, ReferenceSyntax = "symbol" };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new ProductionRuleReference { Name = "number1", ReferenceSyntax = "symbol" };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();
    }

    [Fact]
    public void CompareSimpleProductionRule()
    {
        var p1 = new ProductionRule { Name = "number", Type = ProductionRuleType.Group, EvaluationType = EvaluationType.Sequence };
        var p2 = new ProductionRule { Name = "number", Type = ProductionRuleType.Group, EvaluationType = EvaluationType.Sequence, Index = 1 };
        (p1 == p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();

        p2 = new ProductionRule { Name = "number", Type = ProductionRuleType.Root, EvaluationType = EvaluationType.Sequence };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new ProductionRule { Name = null!, Type = ProductionRuleType.Group, EvaluationType = EvaluationType.Sequence };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new ProductionRule { Name = "number1", Type = ProductionRuleType.Group, EvaluationType = EvaluationType.Sequence };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new ProductionRule { Name = "number", Type = ProductionRuleType.Group, EvaluationType = EvaluationType.Or };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();
    }

    [Fact]
    public void CompareProductionRule()
    {
        var p1 = new ProductionRule
        {
            Name = "number",
            Type = ProductionRuleType.Repeat,
            Children = new IMetaSyntax[]
            {
                new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex },
                new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol" },
            },
        };

        var p2 = new ProductionRule
        {
            Name = "number",
            Type = ProductionRuleType.Repeat,
            Children = new IMetaSyntax[]
            {
                new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Index = 1 },
                new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol", Index = 1 },
            },
        };

        (p1 == p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();

        p2 = new ProductionRule
        {
            Name = "number",
            Type = ProductionRuleType.Repeat,
            Children = new IMetaSyntax[]
            {
                new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol" },
            },
        };

        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new ProductionRule
        {
            Name = "number",
            Type = ProductionRuleType.Repeat,
            Children = new IMetaSyntax[]
            {
                new TerminalSymbol { Name = "number1", Text = "[+-]?[0-9]+", Type = TerminalType.Regex },
                new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol" },
            },
        };

        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();
    }
}
