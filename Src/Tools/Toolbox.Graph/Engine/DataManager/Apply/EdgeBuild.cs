using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class EdgeBuild
{
    public static Task<Option> Build(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        map.NotNull();
        entry.NotNull();

        switch (entry.SourceName, entry.Action)
        {
            case (ChangeSource.Edge, ChangeOperation.Add):
                return Add(map, entry, logger).ThrowOnError().ToTaskResult();

            case (ChangeSource.Edge, ChangeOperation.Delete):
                return Delete(map, entry, logger).ThrowOnError().ToTaskResult();

            case (ChangeSource.Edge, ChangeOperation.Update):
                return Update(map, entry, logger).ThrowOnError().ToTaskResult();
        }

        return new Option(StatusCode.NotFound).ToTaskResult();
    }

    private static Option Add(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        GraphEdge edge = entry.After?.ToObject<GraphEdge>() ?? throw new InvalidOperationException("Failed to deserialize edge from entry");
        edge.Validate().ThrowOnError("Validation failure");

        var addOption = map.Edges.Add(edge);
        if (addOption.IsError())
        {
            logger.LogError("Build: Failed to add edge entry={entry}", entry);
            return StatusCode.BadRequest;
        }

        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static Option Delete(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        GraphEdge edge = entry.After?.ToObject<GraphEdge>() ?? throw new InvalidOperationException("Failed to deserialize edge from entry");
        edge.Validate().ThrowOnError("Validation failure");

        var pk = edge.GetPrimaryKey();
        var removeOption = map.Edges.Remove(pk);
        if (removeOption.IsError())
        {
            logger.LogError("Build: Failed to remove edge pk={pk}, entry={entry}", pk, entry);
            return StatusCode.BadRequest;
        }

        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }

    private static Option Update(GraphMap map, DataChangeEntry entry, ILogger logger)
    {
        if (entry.Before == null) throw new InvalidOperationException($"{entry.Action} command does not have 'Before' DataETag data");

        GraphEdge edge = entry.Before?.ToObject<GraphEdge>() ?? throw new InvalidOperationException("Failed to deserialize edge from entry");
        edge.Validate().ThrowOnError("Validation failure");

        var setOption = map.Edges.Set(edge);
        if (setOption.IsError()) return logger.LogStatus(setOption, "Failed to update edge");

        logger.LogDebug("Build: restored edge entity={entity}", entry);
        map.SetLastLogSequenceNumber(entry.LogSequenceNumber);
        return StatusCode.OK;
    }
}
