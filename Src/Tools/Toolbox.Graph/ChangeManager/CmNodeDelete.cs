using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record CmNodeDelete : IChangeLog
{
    public CmNodeDelete(GraphNode oldValue) => CurrentValue = oldValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode CurrentValue { get; }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Nodes[CurrentValue.Key] = CurrentValue;
        graphContext.Context.LogInformation("Rollback: restored node logKey={logKey}, Node key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
