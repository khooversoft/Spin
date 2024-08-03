using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.LangTools;
using Toolbox.Logging;
using Toolbox.Test.LangTools.Meta;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class SyntaxParseTests
{
    private readonly ITestOutputHelper _output;
    private MetaSyntaxRoot? _root;

    public SyntaxParseTests(ITestOutputHelper output) => _output = output.NotNull();

    [Fact]
    public void SimpleGraphRuleParse()
    {
        var services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(_output.WriteLine);
                x.AddDebug();
                x.AddConsole();
            })
            .BuildServiceProvider();

        var logger = services.GetService<ILogger<SyntaxParser>>().NotNull();
        var context = new ScopeContext(logger);

        var root = GetSyntaxRoot();
        var parser = new SyntaxParser(root);

        string rawData = "add node key=node99, newTags;";
        var result = parser.Parse(rawData, context);
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
