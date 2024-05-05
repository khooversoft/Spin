using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphDeleteCommand
{
    public static Option<IGraphQL> Parse(Stack<LangNode> stack)
    {
        if (!stack.TryPeek(out var cmd) || cmd.SyntaxNode.Name != "delete") return (StatusCode.NotFound, $"Command delete not found");
        stack.Pop();

        var selectListOption = GraphSelectCommand.Parse(stack);
        if (selectListOption.IsError()) return selectListOption.ToOptionStatus<IGraphQL>();

        GsSelect gsSelect = selectListOption.Return();
        if (gsSelect.Search.Length == 0) return (StatusCode.BadRequest, "No search for delete");

        switch (gsSelect.Search.Last())
        {
            case GraphNodeSearch:
                return new GsNodeDelete
                {
                    Search = gsSelect.Search,
                };

            case GraphEdgeSearch:
                return new GsEdgeDelete
                {
                    Search = gsSelect.Search,
                };

            case object v: throw new UnreachableException($"Unknown search type {v.GetType().FullName}");
        }

        return (StatusCode.BadRequest, "Unknown language node");
    }
}