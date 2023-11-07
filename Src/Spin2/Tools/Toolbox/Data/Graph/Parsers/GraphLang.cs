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
                    var nodeParse = GraphSelect.Parse(stack, "select");
                    if (nodeParse.IsError()) return nodeParse;

                    list.AddRange(nodeParse.Return());
                    break;

                case { SyntaxNode.Name: "add" }:
                    var addParse = GraphAdd.Parse(stack);
                    if (addParse.IsError()) return addParse;

                    list.AddRange(addParse.Return());
                    break;

                case { SyntaxNode.Name: "delete" }:
                    var deleteParse = GraphDelete.Parse(stack);
                    if (deleteParse.IsError()) return deleteParse;

                    list.AddRange(deleteParse.Return());
                    break;

                case { SyntaxNode.Name: "update" }:
                    var updateParse = GraphUpdate.Parse(stack);
                    if (updateParse.IsError()) return updateParse;

                    list.AddRange(updateParse.Return());
                    break;
            }
        }

        return list;
    }
}

