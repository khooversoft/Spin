using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class NodeCompensate
{
    public static Task<Option> Compensate(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        map.NotNull();
        entry.NotNull();
        logger.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Node, ChangeOperation.Add):
                return UndoAdd(map, entry, logger).ThrowOnError().ToTaskResult();

            case (ChangeSource.Node, ChangeOperation.Delete):
            case (ChangeSource.Node, ChangeOperation.Update):
                return UndoUpdate(map, entry, logger).ThrowOnError().ToTaskResult();
        }

        return new Option(StatusCode.NotFound).ToTaskResult();
    }

    private static Option UndoAdd(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        if (entry.After == null) throw new ArgumentNullException($"{entry.Action} command does not have 'After' DataETag data");
        GraphNode node = entry.After.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        if (map.NotNull().Nodes.Remove(node.Key).IsError())
        {
            logger.LogError("Rollback: Failed to remove node key={key}, entry={entry}", node.Key, entry);
            return (StatusCode.Conflict, $"Failed to remove node key={node.Key}");
        }

        logger.LogDebug("Rollback: removed node key={key}, entry={entry}", node.Key, entry);
        return StatusCode.OK;
    }

    private static Option UndoUpdate(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        GraphNode node = entry.Before.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        map.Nodes[node.Key] = node;

        logger.LogDebug("Rollback: restored node nodeKey={nodeKey}, entity={entity}", node.Key, entry);
        return StatusCode.OK;
    }
}
