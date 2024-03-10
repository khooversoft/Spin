using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class NodeChange : IChangeLog
{
    private readonly string _nodeKey;
    private readonly GraphNode? _oldValue;

    public NodeChange(string nodeKey, GraphNode? oldValue)
    {
        _nodeKey = nodeKey.NotEmpty();
        _oldValue = oldValue;
    }

    public Guid LogKey { get; } = Guid.NewGuid();

    public Option Undo(GraphChangeContext graphContext)
    {
        graphContext.NotNull();

        if (_oldValue != null)
        {
            graphContext.Map.Nodes[_nodeKey] = _oldValue;
            graphContext.Context.LogInformation("Rollback Node: restored node logKey={logKey}, key={key}, value={value}", LogKey, _nodeKey, _oldValue.ToJson());
            return StatusCode.OK;
        }

        var removeStatus = graphContext.Map.Nodes.Remove(_nodeKey);
        if (!removeStatus)
        {
            graphContext.Context.LogError("Rollback: logKey={logKey}, Failed to remove node key={key}", LogKey, _nodeKey);
            return (StatusCode.Conflict, $"Failed to remove node key={_nodeKey}");
        }

        graphContext.Context.LogInformation("Rollback: removed node logKey={logKey}, Node key={key}", LogKey, _nodeKey);
        return StatusCode.OK;
    }
}
