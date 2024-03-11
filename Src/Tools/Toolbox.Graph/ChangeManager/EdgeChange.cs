using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class EdgeChange : IChangeLog
{
    private readonly Guid _edgeKey;
    private readonly GraphEdge? _oldValue;

    public EdgeChange(Guid edgeKey, GraphEdge? oldValue)
    {
        _edgeKey = edgeKey;
        _oldValue = oldValue;
    }

    public Guid LogKey { get; } = Guid.NewGuid();

    public Option Undo(GraphChangeContext graphContext)
    {
        graphContext.NotNull();

        if (_oldValue != null)
        {
            graphContext.Map.Edges[_oldValue.Key] = _oldValue;
            graphContext.Context.LogInformation("Rollback Edge: restored edge logKey={logKey}, edgeKey={key}, value={value}", LogKey, _edgeKey, _oldValue.ToJson());
            return StatusCode.OK;
        }

        var removeStatus = graphContext.Map.Edges.Remove(_edgeKey);
        if (!removeStatus)
        {
            graphContext.Context.LogError("Rollback Edge: logKey={logKey}, Failed to remove node key={key}", _edgeKey);
            return (StatusCode.Conflict, $"Failed to remove edge edgeKey={_edgeKey}");
        }

        graphContext.Context.LogInformation("Rollback Edge: removed edge logKey={logKey}, Edge edgeKey={key} ", LogKey, _edgeKey);
        return StatusCode.OK;
    }
}
