using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

public class SelectMultipleNodesEdgesTests
{
    private readonly ITestOutputHelper _output;
    private readonly IHost _host;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ILogger _logger;

    public SelectMultipleNodesEdgesTests(ITestOutputHelper output)
    {
        _output = output.NotNull();

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _logger = _host.Services.GetRequiredService<ILogger<BatchTests>>();
        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _root);
    }

    [Fact]
    public void SelectCommand()
    {
        var parse = _parser.Parse("upsert node key=k1 set t1, entity { entityBase64 }, t2=v3, t3, data { base64 } ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

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

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

}
