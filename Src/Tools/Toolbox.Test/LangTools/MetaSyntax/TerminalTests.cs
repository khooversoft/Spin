using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class TerminalTests
{
    [Fact]
    public void TerminalSymbol()
    {
        string schemaText = new[]
        {
            "number = regex '^[+-]?[0-9]+$' ;",
            "alias = number ;"
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().Should().BeTrue();

        var parser = new SyntaxParser(schema);

        var parse = parser.Parse("3", NullScopeContext.Instance);
        parse.IsOk().Should().BeTrue();

        var root = parse.Return().SyntaxTree;

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(root).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
{
    new SyntaxPair
    {
        Token = new TokenValue('3'),
        MetaSyntax = new TerminalSymbol { Name = "number", Text = "^[+-]?[0-9]+$", Type = TerminalType.Regex },
    }
}


        (root == expectedTree).Should().BeTrue();
    }
}
