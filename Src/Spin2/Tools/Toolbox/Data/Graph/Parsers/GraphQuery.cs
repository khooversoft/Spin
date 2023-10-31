using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data.Graph;

/// <summary>
/// Query graph
/// 
/// example: toKey = 'value1' && fromKey = 'value2' && tags has 't2=v2' && tokey match 'schema:*'
/// 
/// Symbols:
///   NodeKey, NodeTags, FromKey, ToKey, FromKey, EdgeType, Tags
///  
/// </summary>
public class GraphQuery
{
    private readonly IReadOnlyList<Func<IGraphNode<string>, bool>> _nodeTests;
    private readonly IReadOnlyList<Func<IGraphEdge<string>, bool>> _edgeTests;

    private GraphQuery(IEnumerable<Func<IGraphNode<string>, bool>> nodes, IEnumerable<Func<IGraphEdge<string>, bool>> edges)
    {
        _nodeTests = nodes.NotNull().ToArray();
        _edgeTests = edges.NotNull().ToArray();
    }

    public bool IsMatch(IGraphNode<string> node) => _nodeTests.All(x => x(node));
    public bool IsMatch(IGraphEdge<string> edge) => _edgeTests.All(x => x(edge));

    public static Option<GraphQuery> Create(string query)
    {
        var parseOption = GraphCmdParser.Parse(query, "=", "has", "match", "&&");
        if (parseOption.IsError()) return parseOption.ToOptionStatus<GraphQuery>();

        var stack = parseOption.Return().Reverse().ToStack();

        var cmds = new Sequence<QueryCmd>();
        while (stack.Count > 0)
        {
            if (!stack.TryPop(out var cmd)) return StatusCode.BadRequest;
            cmds += cmd with { Symbol = cmd.Symbol?.ToLower() };

            if (stack.TryPop(out var andCmd))
            {
                if (andCmd.Opr != QueryOpr.And) return StatusCode.BadRequest;
            }
        }

        var nodes = new Sequence<Func<IGraphNode<string>, bool>>();
        var edges = new Sequence<Func<IGraphEdge<string>, bool>>();

        foreach (var item in cmds)
        {
            switch (item)
            {
                case { Opr: QueryOpr.Equal, Symbol: "nodekey" } v:
                    nodes += x => GraphEdgeTool.IsKeysEqual(x.Key, v.Value.NotNull());
                    break;

                case { Opr: QueryOpr.Equal, Symbol: "fromkey" } v:
                    nodes += x => GraphEdgeTool.IsKeysEqual(x.Key, v.Value.NotNull());
                    break;

            }
        }








        return new GraphQuery(nodes, edges);
    }
}
