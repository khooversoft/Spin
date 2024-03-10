using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record NodeDelete : IChangeLog
{
    private readonly GraphNode _oldValue;
    public NodeDelete(GraphNode oldValue) => _oldValue = oldValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();

    public Option Undo(GraphChangeContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Nodes[_oldValue.Key] = _oldValue;
        graphContext.Context.LogInformation("Rollback: restored node logKey={logKey}, Node key={key}, value={value}", LogKey, _oldValue.Key, _oldValue.ToJson());
        return StatusCode.OK;
    }
}
