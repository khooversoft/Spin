using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class NodeBuild
{
    public static Task<Option> Build(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        map.NotNull();
        entry.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Node, ChangeOperation.Add):
                return Add(map, entry, context).ThrowOnError().ToTaskResult();

            case (ChangeSource.Node, ChangeOperation.Delete):
                return Delete(map, entry, context).ThrowOnError().ToTaskResult();

            case (ChangeSource.Node, ChangeOperation.Update):
                return Update(map, entry, context).ThrowOnError().ToTaskResult();
        }

        return new Option(StatusCode.NotFound).ToTaskResult();
    }

    private static Option Add(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        if (entry.After == null) throw new ArgumentNullException($"{entry.Action} command does not have 'After' DataETag data");
        GraphNode node = entry.After.Value.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        if (map.Nodes.Add(node).IsError())
        {
            context.LogError("Build: Failed to remove node key={key}, entry={entry}", node.Key, entry);
            return (StatusCode.Conflict, $"Failed to add node key={node.Key}");
        }

        context.LogDebug("Build: add node key={key}, entry={entry}", node.Key, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static Option Delete(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        if (entry.Before == null) throw new ArgumentNullException($"{entry.Action} command does not have 'After' DataETag data");
        GraphNode node = entry.Before.Value.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        if (map.Nodes.Remove(node.Key).IsError())
        {
            context.LogError("Build: Failed to remove node key={key}, entry={entry}", node.Key, entry);
            return (StatusCode.Conflict, $"Failed to remove node key={node.Key}");
        }

        context.LogDebug("Build: removed node key={key}, entry={entry}", node.Key, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static Option Update(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        GraphNode node = entry.Before.Value.ToObject<GraphNode>() ?? throw new InvalidOperationException("Failed to deserialize node from entry");
        node.Validate().ThrowOnError("Validation failure");

        var setOption = map.Nodes.Set(node);
        if (setOption.IsError()) return setOption.LogStatus(context, "Failed to set node");

        context.LogDebug("Build: restored node nodeKey={nodeKey}, entity={entity}", node.Key, entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }
}
