using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphQL
{
}

public static class GraphLang
{
    private static readonly ILangRoot _root = GraphLangGrammer.Root;

    public static Option<IReadOnlyList<IGraphQL>> Parse(string rawData)
    {
        LangResult langResult = _root.Parse(rawData);
        if (langResult.IsError()) return new Option<IReadOnlyList<IGraphQL>>(langResult.StatusCode, langResult.Error);

        Stack<LangNode> stack = langResult.LangNodes.NotNull().Reverse().ToStack();
        var list = new List<IGraphQL>();

        while (stack.TryPeek(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "select" }:
                    var nodeParse = GraphSelectCommand.Parse(stack, "select");
                    if (nodeParse.IsError()) return nodeParse;

                    var select = new GraphSelect { Search = nodeParse.Return() };
                    list.Add(select);
                    break;

                case { SyntaxNode.Name: "add" }:
                    var addParse = GraphAddCommand.Parse(stack);
                    if (addParse.IsError()) return addParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

                    list.Add(addParse.Return());
                    break;

                case { SyntaxNode.Name: "delete" }:
                    var deleteParse = GraphDeleteCommand.Parse(stack);
                    if (deleteParse.IsError()) return deleteParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

                    list.Add(deleteParse.Return());
                    break;

                case { SyntaxNode.Name: "update" }:
                    var updateParse = GraphUpdateCommand.Parse(stack);
                    if (updateParse.IsError()) return updateParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();

                    list.Add(updateParse.Return());
                    break;
            }
        }

        return list;
    }
}

