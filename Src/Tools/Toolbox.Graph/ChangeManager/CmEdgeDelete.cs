using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record CmEdgeDelete : IChangeLog
{
    public CmEdgeDelete(GraphEdge oldValue) => CurrentValue = oldValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    private GraphEdge CurrentValue { get; }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Add(CurrentValue);
        graphContext.Context.LogInformation("Rollback: restored edge logKey={logKey}, Edge key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
