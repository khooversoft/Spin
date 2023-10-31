using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.Tokenizer;

public class LangSimpleAssignmentTests
{
    [Fact]
    public void InvalidLangSyntax()
    {
        // Format: s = v

        var root = new LsRoot() + new LsValue("lvalue") + new LsToken("=", "equal") + new LsValue("rvalue");

        var lines = new[] { "s=", "=5", "s 5", "this is wrong", "no" };

        foreach (var test in lines)
        {
            Option<LangNodes> tree = LangParser.Parse(root, test);
            tree.Should().NotBeNull();
            tree.IsError().Should().BeTrue();
        }
    }

    [Fact]
    public void LangSyntax()
    {
        // Format: s = v

        var roots = new[]
        {
            new LsRoot() + new LsValue("lvalue") + new LsToken("=", "equal") + new LsValue("rvalue"),
            new LsRoot() + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue"),
            new LsRoot()
            {
                new LsValue("lvalue"),
                new LsToken("=", "equal"),
                new LsValue("rvalue"),
            },
        };

        var testFor = new List<IQueryResult>()
        {
            new QueryResult<LsValue>("s","lvalue"),
            new QueryResult<LsToken>("=", "equal"),
            new QueryResult<LsValue>("5", "rvalue"),
        };

        var tests = new QueryTest[]
        {
            new QueryTest { RawData = "s=5", Results = testFor },
            new QueryTest { RawData = "s= 5", Results = testFor },
            new QueryTest { RawData = "s =5", Results = testFor },
            new QueryTest { RawData = "s =    5", Results = testFor },
        };

        foreach (var root in roots)
        {
            foreach (var test in tests)
            {
                Option<LangNodes> tree = root.Parse(test.RawData);

                tree.Should().NotBeNull();
                tree.IsOk().Should().BeTrue(tree.ToString());

                LangNodes nodes = tree.Return();
                test.Results.Count.Should().Be(nodes.Children.Count);

                var zip = nodes.Children.Zip(test.Results);
                zip.ForEach(x => x.Second.Test(x.First));
            }
        }
    }

    [Fact]
    public void LangSyntaxWithRepeat()
    {
        var root = new LsRoot()
            + (new LsRepeat("valueEqual") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue") + new LsToken(";", true));

        var tests = new QueryTest[]
        {
            new QueryTest { RawData = "key='string value'", Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
            } },
            new QueryTest { RawData = "key='string value';", Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsToken>(";"),
            } },
            new QueryTest { RawData = "key='string value';name=v1;", Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsToken>(";"),
                new QueryResult<LsValue>("name","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("v1", "rvalue"),
                new QueryResult<LsToken>(";"),
            } },
            new QueryTest { RawData = "key='string value';name=v1", Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsToken>(";"),
                new QueryResult<LsValue>("name","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("v1", "rvalue"),
            } },
            new QueryTest { RawData = "key='string value';name=v1;second=v3", Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsToken>(";"),
                new QueryResult<LsValue>("name","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("v1", "rvalue"),
                new QueryResult<LsToken>(";"),
                new QueryResult<LsValue>("second","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("v3", "rvalue"),
            } },
            new QueryTest { RawData = "key=;name=v1;second=v3", Results = new List<IQueryResult>() },
            new QueryTest { RawData = "key='string value';name=;second=v3", Results = new List<IQueryResult>() },
            new QueryTest { RawData = "key='string value' name=v1;second=v3", Results = new List<IQueryResult>() },
            new QueryTest { RawData = "key='string value';name=v1;=v3", Results = new List<IQueryResult>() },
        };

        foreach (var test in tests)
        {
            Option<LangNodes> tree = root.Parse(test.RawData);

            tree.Should().NotBeNull();
            var pass = tree switch
            {
                var v when v.IsOk() && test.Results.Count > 0 => true,
                var v when v.IsError() && test.Results.Count == 0 => true,
                _ => false,
            };
            pass.Should().Be(true, $"tree={tree.ToString()}, {test.RawData}");
            if (test.Results.Count == 0) continue;

            LangNodes nodes = tree.Return();
            test.Results.Count.Should().Be(nodes.Children.Count);

            var zip = nodes.Children.Zip(test.Results);
            zip.ForEach(x => x.Second.Test(x.First));
        }
    }


    private interface IQueryResult
    {
        void Test(LangNode node);
    }

    private record QueryTest
    {
        public string RawData { get; init; } = null!;
        public List<IQueryResult> Results { get; init; } = null!;
    }

    private record QueryResult<T> : IQueryResult
    {
        public QueryResult(string value, string? name = null)
        {
            Value = value;
            Name = name;
        }
        public string Value { get; init; } = null!;
        public string? Name { get; }

        public void Test(LangNode node)
        {
            (node.SyntaxNode is T).Should().BeTrue();
            node.SyntaxNode.Name.Should().Be(Name);
            node.Value.Should().Be(Value);
        }
    }
}
