using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Test.LangTools.Meta;

public class ModelEqualTests
{
    [Fact]
    public void CompareTerminal()
    {
        var p1 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        var p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"], Index = 1 };
        (p1 == p2).BeTrue();
        p1.Equals(p2).BeTrue();

        // Name
        p2 = new TerminalSymbol { Name = null!, Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        // Name
        p2 = new TerminalSymbol { Name = "numberx", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        // Text
        p2 = new TerminalSymbol { Name = "number", Text = "x[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1"] };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        // Terminal type
        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Token, Tags = ["t1"] };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        // Tags
        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t2"] };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex, Tags = ["t1", "t2"] };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();
    }

    [Fact]
    public void CompareProductionRuleReference()
    {
        var p1 = new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol" };
        var p2 = new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol", Index = 1 };
        (p1 == p2).BeTrue();
        p1.Equals(p2).BeTrue();

        p2 = new ProductionRuleReference { Name = null!, ReferenceSyntax = "symbol" };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        p2 = new ProductionRuleReference { Name = "number1", ReferenceSyntax = "symbol" };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();
    }

    [Fact]
    public void CompareSimpleProductionRule()
    {
        var p1 = new ProductionRule { Name = "number", Type = ProductionRuleType.Or, };
        var p2 = new ProductionRule { Name = "number", Type = ProductionRuleType.Or, Index = 1 };
        (p1 == p2).BeTrue();
        p1.Equals(p2).BeTrue();

        p2 = new ProductionRule { Name = "number", Type = ProductionRuleType.Sequence, };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        p2 = new ProductionRule { Name = null!, Type = ProductionRuleType.Or };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        p2 = new ProductionRule { Name = "number1", Type = ProductionRuleType.Sequence };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

        p2 = new ProductionRule { Name = "number", Type = ProductionRuleType.Repeat };
        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();
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

        (p1 == p2).BeTrue();
        p1.Equals(p2).BeTrue();

        p2 = new ProductionRule
        {
            Name = "number",
            Type = ProductionRuleType.Repeat,
            Children = new IMetaSyntax[]
            {
                new ProductionRuleReference { Name = "number", ReferenceSyntax = "symbol" },
            },
        };

        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();

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

        (p1 == p2).BeFalse();
        p1.Equals(p2).BeFalse();
    }
}
