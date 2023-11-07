using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.LangTools;
using Toolbox.Types;
using System.Diagnostics;

namespace Toolbox.Data;

public static class GraphDelete
{
    public static Option<IReadOnlyList<IGraphQL>> Parse(Stack<LangNode> stack)
    {
        var selectListOption = GraphSelect.Parse(stack, "delete");
        if (selectListOption.IsError()) return selectListOption;

        IReadOnlyList<IGraphQL> selectList = selectListOption.Return();

        var list = selectList
            .Select(x => x switch
            {
                GraphNodeSelect v => (IGraphQL)new GraphNodeDelete
                {
                    Key = v.Key,
                    Tags = v.Tags,
                    Alias = v.Alias
                },
                GraphEdgeSelect v => (IGraphQL)new GraphEdgeDelete
                {
                    NodeKey = v.NodeKey,
                    FromKey = v.FromKey,
                    ToKey = v.ToKey,
                    EdgeType = v.EdgeType,
                    Tags = v.Tags,
                    Alias = v.Alias,
                    Direction = v.Direction,
                },

                var v => throw new UnreachableException($"Invalid type={v.GetType().FullName}"),
            })
            .ToArray();

        return list;
    }
}