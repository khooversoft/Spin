using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        _oldValue = oldValue.NotNull();
    }

    public Guid LogKey { get; } = Guid.NewGuid();

    public Option Undo(GraphChangeContext graphContext)
    {
        graphContext.NotNull();

        if (_oldValue != null)
        {
            graphContext.Map.Edges[_edgeKey] = _oldValue;
            graphContext.Context.LogInformation("Rollback Edge: logKey={logKey}, edgeKey={key}, restored value={value}", LogKey, _edgeKey, _oldValue.ToJson());
            return StatusCode.OK;
        }

        var removeStatus = graphContext.Map.Edges.Remove(_edgeKey);
        if (!removeStatus)
        {
            graphContext.Context.LogError("Rollback Edge: logKey={logKey}, Failed to remove node key={key}", _edgeKey);
            return (StatusCode.Conflict, $"Failed to remove node edgeKey={_edgeKey}");
        }

        graphContext.Context.LogInformation("Rollback Edge: logKey={logKey}, Edge edgeKey={key} removed", LogKey, _edgeKey);
        return StatusCode.OK;
    }
}
