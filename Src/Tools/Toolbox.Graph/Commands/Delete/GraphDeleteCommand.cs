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

        bool force = stack.TryPeek(out var forceCmd) switch
        {
            true when forceCmd.SyntaxNode.Name == "force" => true.Action(_ => stack.Pop()),
            _ => false,
        };

        var selectListOption = GraphSelectCommand.Parse(stack);
        if (selectListOption.IsError()) return selectListOption.ToOptionStatus<IGraphQL>();

        IReadOnlyList<IGraphQL> selectList = selectListOption.Return();
        if (selectList.Count == 0) return (StatusCode.BadRequest, "No search for delete");

        switch (selectList.Last())
        {
            case GraphNodeSearch:
                return new GraphNodeDelete
                {
                    Search = selectList,
                    Force = force,
                };

            case GraphEdgeSearch:
                return new GraphEdgeDelete
                {
                    Search = selectList,
                };

            case object v: throw new UnreachableException($"Unknown search type {v.GetType().FullName}");
        }

        return (StatusCode.BadRequest, "Unknown language node");
    }
}