using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Lang;

internal static class LangTestTools
{
    public static void Verify(ITestOutputHelper _output, ILangRoot root, QueryTest test)
    {
        LangResult tree = root.Parse(test.RawData);
        tree.Traces.ForEach(x => _output.WriteLine(x.ToString()));

        tree.Should().NotBeNull();
        var pass = tree switch
        {
            var v when v.IsOk() && test.Results.Count > 0 => true,
            var v when v.IsError() && test.Results.Count == 0 => true,
            _ => false,
        };
        pass.Should().Be(true, test.RawData);
        if (test.Results.Count == 0) return;

        LangNodes nodes = tree.LangNodes.NotNull();
        test.Results.Count.Should().Be(nodes.Children.Count);

        var zip = nodes.Children.Zip(test.Results);
        zip.ForEach(x => x.Second.Test(x.First));
    }
}


internal interface IQueryResult
{
    void Test(LangNode node);
}

internal record QueryTest
{
    public string RawData { get; init; } = null!;
    public List<IQueryResult> Results { get; init; } = null!;
}

internal record QueryResult<T> : IQueryResult
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
        if (Name.IsNotEmpty()) node.SyntaxNode.Name.Should().Be(Name);
        node.Value.Should().Be(Value);
    }
}
