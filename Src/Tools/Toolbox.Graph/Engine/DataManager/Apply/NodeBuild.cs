using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class NodeBuild
{
    public static Task<Option> Build(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        map.NotNull();
        entry.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Node, ChangeOperation.Add):
                return Add(map, entry, logger).ThrowOnError().ToTaskResult();

            case (ChangeSource.Node, ChangeOperation.Delete):
                return Delete(map, entry, logger).ThrowOnError().ToTaskResult();

            case (ChangeSource.Node, ChangeOperation.Update):
                return Update(map, entry, logger).ThrowOnError().ToTaskResult();
        }

        return new Option(StatusCode.NotFound).ToTaskResult();
    }

    private static Option Add(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        if (entry.After == null) throw new ArgumentNullException($"{entry.Action} command does not have 'After' DataETag data");
        GraphNode node = entry.After.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        if (map.Nodes.Add(node).IsError())
        {
            logger.LogError("Build: Failed to remove node key={key}, entry={entry}", node.Key, entry);
            return (StatusCode.Conflict, $"Failed to add node key={node.Key}");
        }

        logger.LogDebug("Build: add node key={key}, entry={entry}", node.Key, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static Option Delete(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        if (entry.Before == null) throw new ArgumentNullException($"{entry.Action} command does not have 'After' DataETag data");
        GraphNode node = entry.Before.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        if (map.Nodes.Remove(node.Key).IsError())
        {
            logger.LogError("Build: Failed to remove node key={key}, entry={entry}", node.Key, entry);
            return (StatusCode.Conflict, $"Failed to remove node key={node.Key}");
        }

        logger.LogDebug("Build: removed node key={key}, entry={entry}", node.Key, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static Option Update(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        GraphNode node = entry.Before.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        var setOption = map.Nodes.Set(node);
        if (setOption.IsError()) return logger.LogStatus(setOption, "Failed to set node");

        logger.LogDebug("Build: restored node nodeKey={nodeKey}, entity={entity}", node.Key, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }
}
