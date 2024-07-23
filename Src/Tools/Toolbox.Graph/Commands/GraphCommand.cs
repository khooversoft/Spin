using System.Collections.Immutable;
using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphCommand
{
    public static async Task<Option<GraphQueryResults>> Execute(IGraphTrxContext graphContext, string graphQuery)
    {
        graphContext.NotNull();

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResults>();
        IReadOnlyList<IGraphQL> commands = result.Return();

        var results = new Sequence<GraphQueryResult>();

        bool write = commands.Any(x => x is GsNodeAdd || x is GsEdgeAdd || x is GsEdgeUpdate || x is GsNodeUpdate || x is GsEdgeDelete || x is GsNodeDelete);

        using (var release = write ? (await graphContext.Map.ReadWriterLock.WriterLockAsync()) : (await graphContext.Map.ReadWriterLock.ReaderLockAsync()))
        {
            foreach (var cmd in commands)
            {
                GraphQueryResult qResult = cmd switch
                {
                    GsNodeAdd addNode => await GraphCommandNode.Add(addNode, graphContext),
                    GsNodeUpdate updateNode => await GraphCommandNode.Update(updateNode, graphContext),
                    GsNodeDelete deleteNode => await GraphCommandNode.Delete(deleteNode, graphContext),

                    GsEdgeAdd addEdge => GraphCommandEdge.Add(addEdge, graphContext),
                    GsEdgeUpdate updateEdge => GraphCommandEdge.Update(updateEdge, graphContext),
                    GsEdgeDelete deleteEdge => GraphCommandEdge.Delete(deleteEdge, graphContext),

                    GsSelect select => await Select(select, graphContext),

                    _ => throw new UnreachableException(),
                };

                results += qResult;

                if (qResult.Status.IsError())
                {
                    graphContext.Context.Location().LogError("Graph batch failed - rolling back: query={graphQuery}, error={error}", graphQuery, qResult.ToString());
                    graphContext.ChangeLog.Rollback();
                    break;
                }
            }
        }

        Option option = results switch
        {
            { Count: 0 } => StatusCode.OK,
            var v when v.Last().Status.IsError() => v.Last().Status,
            _ => StatusCode.OK,
        };

        var mapResult = new GraphQueryResults
        {
            Items = results.ToImmutableArray(),
        };

        return new Option<GraphQueryResults>(mapResult, option.StatusCode, option.Error);
    }

    private static async Task<GraphQueryResult> Select(GsSelect select, IGraphTrxContext graphContext)
    {
        GraphQueryResult searchResult = GraphQuery.Process(graphContext.Map, select.Search);

        var readData = new Sequence<GraphLinkData>();
        var nodes = searchResult.Items.OfType<GraphNode>().ToArray();

        var map = select.ReturnNames
            .Join(
                nodes.SelectMany(x => x.DataMap.Values),
                x => x,
                x => x.Name,
                (o, i) => (key: $"{i.NodeKey}:{i.Name}", dataMap: i),
                StringComparer.OrdinalIgnoreCase
                )
            .ToArray();

        foreach (var data in map)
        {
            var readOption = await graphContext.FileStore.Get(data.dataMap.FileId, graphContext.Context);
            if (readOption.IsError())
            {
                graphContext.Context.LogError("Cannot read fileId={fileId}, error={error}", data.dataMap.FileId, readOption.Error);
                return new GraphQueryResult(CommandType.Select, StatusCode.Conflict);
            }

            var linkData = data.dataMap.ConvertTo(readOption.Return());
            readData += linkData;
        }

        searchResult = searchResult with
        {
            CommandType = CommandType.Select,
            DataLinks = readData.ToImmutableArray(),
        };

        return searchResult;
    }
}
