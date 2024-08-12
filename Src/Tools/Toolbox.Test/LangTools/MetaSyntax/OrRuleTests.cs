using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OrRuleTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public OrRuleTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
{
            "node-sym = 'node' ;",
            "edge-sym = 'edge' ;",
            "addCommand = ( node-sym | edge-sym ) ;"
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public void SimpleOrSymbol()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("node", logger);
        parse.StatusCode.IsOk().Should().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntax = new ProductionRule
                    {
                        Name = "_addCommand-1-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-1-Group-1-node-sym", ReferenceSyntax = "node-sym" },
                            new ProductionRuleReference { Name = "_addCommand-1-Group-3-edge-sym", ReferenceSyntax = "edge-sym" },
                        },
                    },
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("node"),
                            MetaSyntax = new TerminalSymbol { Name = "node-sym", Text = "node", Type = TerminalType.Token },
                        }
                    }
                }
            }
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void SimpleOrSymbolSecond()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("edge", logger);
        parse.StatusCode.IsOk().Should().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntax = new ProductionRule
                    {
                        Name = "_addCommand-1-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-1-Group-1-node-sym", ReferenceSyntax = "node-sym" },
                            new ProductionRuleReference { Name = "_addCommand-1-Group-3-edge-sym", ReferenceSyntax = "edge-sym" },
                        },
                    },
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("edge"),
                            MetaSyntax = new TerminalSymbol { Name = "edge-sym", Text = "edge", Type = TerminalType.Token },
                        }
                    }
                }
            }
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }


    [Fact]
    public void SimpleOrSymbolFail()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("edxxxge", logger);
        parse.StatusCode.IsError().Should().BeTrue();
    }
}
