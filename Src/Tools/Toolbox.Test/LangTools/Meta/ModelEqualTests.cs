using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.LangTools;

namespace Toolbox.Test.LangTools.Meta;

public class ModelEqualTests
{
    [Fact]
    public void CompareTerminal()
    {
        var p1 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Regex = true };
        var p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Regex = true, Index = 1 };
        (p1 == p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();

        p2 = new TerminalSymbol { Name = null!, Text = "[+-]?[0-9]+", Regex = true };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new TerminalSymbol { Name = "numberx", Text = "[+-]?[0-9]+", Regex = true };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new TerminalSymbol { Name = "number", Text = "x[+-]?[0-9]+", Regex = true };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Regex = false };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();
    }

    [Fact]
    public void CompareProductionRuleReference()
    {
        var p1 = new ProductionRuleReference { Name = "number", ReferenceSyntax = new TerminalSymbol() };
        var p2 = new ProductionRuleReference { Name = "number", ReferenceSyntax = new TerminalSymbol(), Index = 1 };
        (p1 == p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();

        p2 = new ProductionRuleReference { Name = null!, ReferenceSyntax = new TerminalSymbol() };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new ProductionRuleReference { Name = "number1", ReferenceSyntax = new TerminalSymbol() };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();
    }

    [Fact]
    public void CompareGroupOperator()
    {
        var p1 = new GroupOperator { Name = "number" };
        var p2 = new GroupOperator { Name = "number", Index = 1 };
        (p1 == p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();

        p2 = new GroupOperator { Name = null! };
        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();

        p2 = new GroupOperator { Name = "number1" };
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

        p2 = new ProductionRule { Name = "number", Type = ProductionRuleType.Group, EvaluationType = EvaluationType.Or};
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
                new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Regex = true },
                new ProductionRuleReference { Name = "number", ReferenceSyntax = new TerminalSymbol() },
            },
        };

        var p2 = new ProductionRule
        {
            Name = "number",
            Type = ProductionRuleType.Repeat,
            Children = new IMetaSyntax[]
            {
                new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Regex = true, Index = 1 },
                new ProductionRuleReference { Name = "number", ReferenceSyntax = new TerminalSymbol(), Index = 1 },
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
                new ProductionRuleReference { Name = "number", ReferenceSyntax = new TerminalSymbol() },
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
                new TerminalSymbol { Name = "number1", Text = "[+-]?[0-9]+", Regex = true },
                new ProductionRuleReference { Name = "number", ReferenceSyntax = new TerminalSymbol() },
            },
        };

        (p1 == p2).Should().BeFalse();
        p1.Equals(p2).Should().BeFalse();
    }
}
