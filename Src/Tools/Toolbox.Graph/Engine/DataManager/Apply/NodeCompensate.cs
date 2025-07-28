using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class NodeCompensate
{
    public static Task<Option> Compensate(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        map.NotNull();
        entry.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Node, ChangeOperation.Add):
                return UndoAdd(map, entry, context).ThrowOnError().ToTaskResult();

            case (ChangeSource.Node, ChangeOperation.Delete):
            case (ChangeSource.Node, ChangeOperation.Update):
                return UndoUpdate(map, entry, context).ThrowOnError().ToTaskResult();
        }

        return new Option(StatusCode.NotFound).ToTaskResult();
    }

    private static Option UndoAdd(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        if (entry.After == null) throw new ArgumentNullException($"{entry.Action} command does not have 'After' DataETag data");
        GraphNode node = entry.After.Value.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        if (map.NotNull().Nodes.Remove(node.Key).IsError())
        {
            context.LogError("Rollback: Failed to remove node key={key}, entry={entry}", node.Key, entry);
            return (StatusCode.Conflict, $"Failed to remove node key={node.Key}");
        }

        context.LogDebug("Rollback: removed node key={key}, entry={entry}", node.Key, entry);
        return StatusCode.OK;
    }
    private static Option UndoUpdate(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        GraphNode node = entry.Before.Value.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        map.Nodes[node.Key] = node;

        context.LogDebug("Rollback: restored node nodeKey={nodeKey}, entity={entity}", node.Key, entry);
        return StatusCode.OK;
    }
}
