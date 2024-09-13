//using System.Diagnostics;
//using Toolbox.Extensions;
//using Toolbox.LangTools;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public static class GraphLang
//{
//    private static readonly ILangRoot _root = GraphLangGrammar.Root;

//    public static Option<IReadOnlyList<IGraphQL>> Parse(string rawData)
//    {
//        LangResult langResult = _root.Parse(rawData);
//        if (langResult.IsError()) return new Option<IReadOnlyList<IGraphQL>>(langResult.StatusCode, langResult.Error);

//        Stack<LangNode> stack = langResult.LangNodes.NotNull().Reverse().ToStack();
//        var list = new List<IGraphQL>();

//        while (stack.TryPeek(out var langNode))
//        {
//            switch (langNode)
//            {
//                case { SyntaxNode.Name: "select" }:
//                    var nodeParse = GraphSelectCommand.Parse(stack, "select");
//                    if (nodeParse.IsError()) return nodeParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

//                    var select = nodeParse.Return();
//                    list.Add(select);
//                    break;

//                case { SyntaxNode.Name: "add" }:
//                    var addParse = GraphAddCommand.Parse(stack);
//                    if (addParse.IsError()) return addParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

//                    list.Add(addParse.Return());
//                    break;

//                case { SyntaxNode.Name: "upsert" }:
//                    var upsertParse = GraphAddCommand.Parse(stack, upsert: true);
//                    if (upsertParse.IsError()) return upsertParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

//                    list.Add(upsertParse.Return());
//                    break;

//                case { SyntaxNode.Name: "delete" }:
//                    var deleteParse = GraphDeleteCommand.Parse(stack);
//                    if (deleteParse.IsError()) return deleteParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

//                    list.Add(deleteParse.Return());
//                    break;

//                case { SyntaxNode.Name: "update" }:
//                    var updateParse = GraphUpdateCommand.Parse(stack);
//                    if (updateParse.IsError()) return updateParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

//                    list.Add(updateParse.Return());
//                    break;
//            }
//        }

//        var reserveWords = list
//            .SelectMany(x => x switch
//            {
//                GsNodeAdd v => v.Tags.Select(x => x.Key),
//                GsEdgeAdd v => v.Tags.Select(x => x.Key),
//                GsEdgeUpdate v => v.Tags.Select(x => x.Key),
//                GsNodeUpdate v => getNodeAndEdges(v.Search),

//                GsSelect v => getNodeAndEdges(v.Search),
//                GsNodeDelete v => getNodeAndEdges(v.Search),
//                GsEdgeDelete v => getNodeAndEdges(v.Search),

//                _ => throw new UnreachableException(),
//            })
//            .Where(GraphLangGrammar.ReserveTokens.Contains)
//            .ToArray();

//        if (reserveWords.Length > 0) return (StatusCode.BadRequest, $"Cannot use reserved token={reserveWords.Join(",")}");

//        return list;

//        IEnumerable<string> getNodeAndEdges(IEnumerable<IGraphQL> items) => items.SelectMany(x => x switch
//        {
//            GraphNodeSearch s => s.Tags.Select(x => x.Key),
//            GraphEdgeSearch s => s.Tags.Select(x => x.Key),

//            _ => throw new UnreachableException(),
//        });
//    }
//}

