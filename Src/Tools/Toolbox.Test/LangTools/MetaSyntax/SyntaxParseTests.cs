using FluentAssertions;
using Toolbox.LangTools;
using Toolbox.Test.LangTools.Meta;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class SyntaxParseTests
{
    private MetaSyntaxRoot? _root;

    [Fact(Skip = "dkdk")]
    public void SimpleGraphRuleParse()
    {
        var root = GetSyntaxRoot();
        var parser = new SyntaxParser(root);

        string rawData = "add node key=node99, newTags;";
        var result = parser.Parse(rawData);
        result.IsOk().Should().BeTrue();
    }

    private MetaSyntaxRoot GetSyntaxRoot()
    {
        return _root ??= read();

        static MetaSyntaxRoot read()
        {
            string metaSyntax = MetaTestTool.ReadGraphLanauge();
            var root = MetaParser.ParseRules(metaSyntax);
            root.StatusCode.IsOk().Should().BeTrue();
            return root;
        }
    }
}
