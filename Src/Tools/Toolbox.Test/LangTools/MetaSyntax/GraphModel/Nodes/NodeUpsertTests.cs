using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Nodes;

public class NodeUpsertTests
{
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;

    public NodeUpsertTests(ITestOutputHelper output)
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services, _root);
    }

    [Fact]
    public void UpsertCommand()
    {
        var parse = _parser.Parse("upsert node key=k1 ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("upsert"), Name = "upsert-sym" },
            new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "key-value" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void UpsertCommandWithTagsTwoData()
    {
        var parse = _parser.Parse("upsert node key=k1 set t1, entity { entityBase64 }, t2=v3, t3, data { base64 } ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("upsert"), Name = "upsert-sym" },
            new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
            new SyntaxPair { Token = new TokenValue("entityBase64"), Name = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v3"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t3"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("data"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
            new SyntaxPair { Token = new TokenValue("base64"), Name = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}
