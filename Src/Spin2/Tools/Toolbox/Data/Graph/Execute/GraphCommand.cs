﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data.Graph.Execute;

public class GraphCommand
{
    private readonly GraphMap _map;
    public GraphCommand(GraphMap map) => _map = map.NotNull();

    public Option<GraphMap> Execute(string rawData)
    {
        var map = _map.Clone();

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(rawData);
        if (result.IsError()) return result.ToOptionStatus<GraphMap>();

        IReadOnlyList<IGraphQL> commands = result.Return();

        foreach (var cmd in commands)
        {
            switch (cmd)
            {
                case GraphNodeAdd addNode:
                    Option addNodeOption = AddNode(addNode, map);
                    if (addNodeOption.IsError()) return addNodeOption.ToOptionStatus<GraphMap>();
                    break;

                case GraphEdgeAdd addEdge:
                    Option addEdgeOption = AddEdge(addEdge, map);
                    if (addEdgeOption.IsError()) return addEdgeOption.ToOptionStatus<GraphMap>();
                    break;

                case GraphEdgeUpdate updateEdge:
                    Option updateEdgeNodeOption = UpdateEdge(updateEdge, map);
                    if (updateEdgeNodeOption.IsError()) return updateEdgeNodeOption.ToOptionStatus<GraphMap>();
                    break;

                case GraphNodeUpdate updateNode:
                    Option updateNodeOption = UpdateNode(updateNode, map);
                    if (updateNodeOption.IsError()) return updateNodeOption.ToOptionStatus<GraphMap>();
                    break;

                case GraphEdgeDelete deleteEdge:
                    Option deleteEdgeOption = DeleteEdge(deleteEdge, map);
                    if (deleteEdgeOption.IsError()) return deleteEdgeOption.ToOptionStatus<GraphMap>();
                    break;

                case GraphNodeDelete deleteNode:
                    Option deleteNodeOption = DeleteNode(deleteNode, map);
                    if (deleteNodeOption.IsError()) return deleteNodeOption.ToOptionStatus<GraphMap>();
                    break;

                case GraphNodeSelect selectNode:
                    Option selectNodeOption = SelectNode(selectNode, map);
                    if (selectNodeOption.IsError()) return selectNodeOption.ToOptionStatus<GraphMap>();
                    break;

                case GraphEdgeSelect selectEdge:
                    Option selectEdgeOption = SelectEdge(selectEdge, map);
                    if (selectEdgeOption.IsError()) return selectEdgeOption.ToOptionStatus<GraphMap>();
                    break;
            }
        }

        return map;
    }
    private Option AddNode(GraphNodeAdd addNode, GraphMap map)
    {
        var graphNode = new GraphNode
        {
            Key = addNode.Key,
            Tags = addNode.Tags,
        };

        return map.Nodes.Add(graphNode);
    }

    private Option AddEdge(GraphEdgeAdd addEdge, GraphMap map)
    {
        var graphEdge = new GraphEdge
        {
            FromKey = addEdge.FromKey,
            ToKey = addEdge.ToKey,
            EdgeType = addEdge.EdgeType ?? "default",
            Tags = new Tags(addEdge.Tags),
        };

        return map.Edges.Add(graphEdge);
    }

    private Option UpdateEdge(GraphEdgeUpdate updateEdge, GraphMap map)
    {
        var searchResult = map.Query().Process(updateEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return StatusCode.NoContent;

        _map.Edges.Update(edges, x => x with
        {
            EdgeType = updateEdge.EdgeType ?? x.EdgeType,
            Tags = x.Tags.Set(updateEdge.Tags),
        });

        return StatusCode.OK;
    }

    private Option UpdateNode(GraphNodeUpdate updateNode, GraphMap map)
    {
        var searchResult = map.Query().Process(updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return StatusCode.NoContent;

        _map.Nodes.Update(nodes, x => x with
        {
            Tags = x.Tags.Set(updateNode.Tags),
        });

        return StatusCode.OK;
    }

    private Option DeleteEdge(GraphEdgeDelete deleteEdge, GraphMap map)
    {
        //var searchResult = map.Query().Process(deleteEdge.Search);

        //IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        //if (edges.Count == 0) return StatusCode.NoContent;
        return default!;
    }

    private Option DeleteNode(GraphNodeDelete deleteNode, GraphMap map)
    {
        throw new NotImplementedException();
    }

    private Option SelectNode(GraphNodeSelect selectNode, GraphMap map)
    {
        throw new NotImplementedException();
    }

    private Option SelectEdge(GraphEdgeSelect selectEdge, GraphMap map)
    {
        throw new NotImplementedException();
    }
}