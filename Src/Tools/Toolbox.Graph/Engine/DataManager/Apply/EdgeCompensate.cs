using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class EdgeCompensate
{
    public static Task<Option> Compensate(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        map.NotNull();
        entry.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Edge, ChangeOperation.Add):
                return UndoAdd(map, entry, context).ThrowOnError().ToTaskResult();

            case (ChangeSource.Edge, ChangeOperation.Delete):
            case (ChangeSource.Edge, ChangeOperation.Update):
                return UndoUpdate(map, entry, context).ThrowOnError().ToTaskResult();
        }

        return new Option(StatusCode.NotFound).ToTaskResult();
    }

    private static Option UndoAdd(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        GraphEdge edge = entry.After?.ToObject<GraphEdge>() ?? throw new InvalidOperationException("Failed to deserialize edge from entry");
        edge.Validate().ThrowOnError("Validation failure");

        var pk = edge.GetPrimaryKey();
        var removeOption = map.Edges.Remove(pk);
        if (removeOption.IsError())
        {
            context.LogError("Rollback Edge: Failed to remove edge pk={pk}, entry={entry}", pk, entry);
            return StatusCode.BadRequest;
        }

        return StatusCode.OK;
    }

    private static Option UndoUpdate(GraphMap map, DataChangeEntry entry, ScopeContext context)
    {
        if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        GraphEdge edge = entry.Before?.ToObject<GraphEdge>() ?? throw new InvalidOperationException("Failed to deserialize edge from entry");
        edge.Validate().ThrowOnError("Validation failure");

        var pk = edge.GetPrimaryKey();
        map.Edges[pk] = edge;

        context.LogDebug("Rollback: restored edge pk={pk}, entity={entity}", pk, entry);
        return StatusCode.OK;
    }
}
