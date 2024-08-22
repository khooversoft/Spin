//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.LangTools;
//using Toolbox.Logging;
//using Toolbox.Test.LangTools.Meta;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

//public class AddNodeTests
//{
//    private readonly ITestOutputHelper _output;
//    private readonly MetaSyntaxRoot _root;
//    private readonly SyntaxParser _parser;
//    private readonly ScopeContext _context;

//    public AddNodeTests(ITestOutputHelper output)
//    {
//        _output = output.NotNull();

//        string metaSyntax = MetaTestTool.ReadGraphLanauge();
//        _root = MetaParser.ParseRules(metaSyntax);
//        _root.StatusCode.IsOk().Should().BeTrue();

//        var services = new ServiceCollection()
//            .AddLogging(x =>
//            {
//                x.AddLambda(_output.WriteLine);
//                x.AddDebug();
//                x.AddConsole();
//            })
//            .BuildServiceProvider();

//        var logger = services.GetService<ILogger<SyntaxParser>>().NotNull();
//        _context = new ScopeContext(logger);

//        _parser = new SyntaxParser(_root);
//    }

//    [Fact]
//    public void AddNodeTest()
//    {
//        string rawData = "add node key=node99 set newTags ;";
//        var parse = _parser.Parse(rawData, _context);
//        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

//        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

//        var expectedTree = new SyntaxTree
//        {
//            Children = new ISyntaxTree[]
//            {
//                new SyntaxTree
//                {
//                    MetaSyntaxName = "addCommand",
//                    Children = new ISyntaxTree[]
//                    {
//                        new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
//                        new SyntaxTree
//                        {
//                            MetaSyntaxName = "_addCommand-3-OrGroup",
//                            Children = new ISyntaxTree[]
//                            {
//                                new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
//                            },
//                        },
//                        new SyntaxTree
//                        {
//                            MetaSyntaxName = "tags",
//                            Children = new ISyntaxTree[]
//                            {
//                                new SyntaxTree
//                                {
//                                    MetaSyntaxName = "tag",
//                                    Children = new ISyntaxTree[]
//                                    {
//                                        new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "tagKey" },
//                                        new SyntaxTree
//                                        {
//                                            MetaSyntaxName = "_tag-3-OptionGroup",
//                                            Children = new ISyntaxTree[]
//                                            {
//                                                new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
//                                                new SyntaxPair { Token = new TokenValue("node99"), MetaSyntaxName = "tagValue" },
//                                            },
//                                        },
//                                    },
//                                },
//                                new SyntaxTree
//                                {
//                                    MetaSyntaxName = "_tags-3-RepeatGroup",
//                                    Children = new ISyntaxTree[]
//                                    {
//                                        new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
//                                        new SyntaxTree
//                                        {
//                                            MetaSyntaxName = "tag",
//                                            Children = new ISyntaxTree[]
//                                            {
//                                                new SyntaxPair { Token = new TokenValue("newTags"), MetaSyntaxName = "tagKey" },
//                                            },
//                                        },
//                                    },
//                                },
//                            },
//                        },
//                        new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
//                    },
//                },
//            },
//        };

//        (parse.SyntaxTree == expectedTree).Should().BeTrue();

//        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
//        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

//        var expectedPairs = new[]
//        {
//            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
//            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
//            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "tagKey" },
//            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
//            new SyntaxPair { Token = new TokenValue("node99"), MetaSyntaxName = "tagValue" },
//            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
//            new SyntaxPair { Token = new TokenValue("newTags"), MetaSyntaxName = "tagKey" },
//            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
//        };

//        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
//    }

//    [Fact]
//    public void AddNodeWithData()
//    {
//        string rawData = "add node key=node99, newTags, t2=v2, data { 'base64 test data' } ;";
//        var parse = _parser.Parse(rawData, _context);
//        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

//        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

//        var expectedTree = new SyntaxTree
//        {
//            Children = new ISyntaxTree[]
//            {
//                new SyntaxTree
//                {
//                    MetaSyntaxName = "addCommand",
//                    Children = new ISyntaxTree[]
//                    {
//                        new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
//                        new SyntaxTree
//                        {
//                            MetaSyntaxName = "_addCommand-3-OrGroup",
//                            Children = new ISyntaxTree[]
//                            {
//                                new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
//                            },
//                        },
//                        new SyntaxTree
//                        {
//                            MetaSyntaxName = "_addCommand-5-OptionGroup",
//                            Children = new ISyntaxTree[]
//                            {
//                                new SyntaxTree
//                                {
//                                    MetaSyntaxName = "tags",
//                                    Children = new ISyntaxTree[]
//                                    {
//                                        new SyntaxTree
//                                        {
//                                            MetaSyntaxName = "tag",
//                                            Children = new ISyntaxTree[]
//                                            {
//                                                new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "tagKey" },
//                                                new SyntaxTree
//                                                {
//                                                    MetaSyntaxName = "_tag-3-OptionGroup",
//                                                    Children = new ISyntaxTree[]
//                                                    {
//                                                        new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
//                                                        new SyntaxPair { Token = new TokenValue("node99"), MetaSyntaxName = "tagValue" },
//                                                    },
//                                                },
//                                            },
//                                        },
//                                        new SyntaxTree
//                                        {
//                                            MetaSyntaxName = "_tags-3-RepeatGroup",
//                                            Children = new ISyntaxTree[]
//                                            {
//                                                new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
//                                                new SyntaxTree
//                                                {
//                                                    MetaSyntaxName = "tag",
//                                                    Children = new ISyntaxTree[]
//                                                    {
//                                                        new SyntaxPair { Token = new TokenValue("newTags"), MetaSyntaxName = "tagKey" },
//                                                    },
//                                                },
//                                            },
//                                        },
//                                    },
//                                },
//                            },
//                        },
//                        new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
//                    },
//                },
//            },
//        };

//        (parse.SyntaxTree == expectedTree).Should().BeTrue();

//        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
//        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

//        var expectedPairs = new[]
//        {
//            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
//            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
//            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "tagKey" },
//            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
//            new SyntaxPair { Token = new TokenValue("node99"), MetaSyntaxName = "tagValue" },
//            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
//            new SyntaxPair { Token = new TokenValue("newTags"), MetaSyntaxName = "tagKey" },
//            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
//        };

//        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
//    }
//}
