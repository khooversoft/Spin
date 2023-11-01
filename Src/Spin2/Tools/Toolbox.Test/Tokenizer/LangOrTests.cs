using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.Tokenizer;

public class LangOrTests
{
    [Fact]
    public void SimpleOrTest()
    {
        var equalValue = new LsRoot() + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
        var valueOnly = new LsRoot() + new LsValue("svalue");

        var root = new LsRoot()
            + (new LsOr("or") + equalValue + valueOnly);

        var tests = new QueryTest[]
        {
            new QueryTest { RawData = "key='string value'", Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
            } },
            new QueryTest { RawData = "key", Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","svalue"),
            } },
            new QueryTest { RawData = "key=", Results = new List<IQueryResult>() },
            new QueryTest { RawData = "=", Results = new List<IQueryResult>() },
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
            pass.Should().Be(true, test.RawData);
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
            (node.SyntaxNode is T).Should().BeTrue($"SyntaxNode={node.SyntaxNode.GetType().Name}, T={typeof(T).Name}");
            node.SyntaxNode.Name.Should().Be(Name);
            node.Value.Should().Be(Value);
        }
    }
}
