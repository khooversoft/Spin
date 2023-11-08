using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class GraphDelete
{
    public static Option<IReadOnlyList<IGraphQL>> Parse(Stack<LangNode> stack)
    {
        var selectListOption = GraphSelect.Parse(stack, "delete");
        if (selectListOption.IsError()) return selectListOption;

        IReadOnlyList<IGraphQL> selectList = selectListOption.Return();
        if (selectList.Count == 0) return (StatusCode.BadRequest, "No search for delete");

        var list = new Sequence<IGraphQL>();

        switch (selectList.Last())
        {
            case GraphNodeSelect:
                list += new GraphNodeDelete
                {
                    Search = selectList,
                };
                break;

            case GraphEdgeSelect:
                list += new GraphEdgeDelete
                {
                    Search = selectList,
                };
                break;

            case object v: throw new UnreachableException($"Unknown search type {v.GetType().FullName}");
        }

        return list;
    }
}