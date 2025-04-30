//using Toolbox.Tools.Should;
//using Toolbox.Extensions;
//using Toolbox.LangTools;
//using Toolbox.Tools;

//namespace Toolbox.Test.LangTools;

//internal static class LangTestTools
//{
//    public static void Verify(ILangRoot root, QueryTest test)
//    {
//        LangResult tree = root.Parse(test.RawData);

//        tree.NotBeNull();
//        var pass = tree switch
//        {
//            var v when v.IsOk() && test.Results.Count > 0 => true,
//            var v when v.IsError() && test.Results.Count == 0 => true,
//            _ => false,
//        };

//        pass.Be(true, test.RawData);
//        if (test.Results.Count == 0) return;

//        LangNodes nodes = tree.LangNodes.NotNull();
//        test.Results.Count.Be(nodes.Children.Count);

//        var zip = nodes.Children.Zip(test.Results);
//        zip.ForEach(x => x.Second.Test(x.First));
//    }
//}


//internal interface IQueryResult
//{
//    void Test(LangNode node);
//}

//internal record QueryTest
//{
//    public string RawData { get; init; } = null!;
//    public List<IQueryResult> Results { get; init; } = null!;
//}

//internal record QueryResult<T> : IQueryResult
//{
//    public QueryResult(string value, string? name = null)
//    {
//        Value = value;
//        Name = name;
//    }
//    public string Value { get; init; } = null!;
//    public string? Name { get; }

//    public void Test(LangNode node)
//    {
//        (node.SyntaxNode is T).BeTrue($"SyntaxNode={node.SyntaxNode.GetType().Name}, T={typeof(T).Name}");
//        if (Name.IsNotEmpty()) node.SyntaxNode.Name.Be(Name);
//        node.Value.Be(Value);
//    }
//}
