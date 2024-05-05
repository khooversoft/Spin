using Toolbox.Types;

namespace Toolbox.Graph;

public interface IChangeLog
{
    Guid LogKey { get; }
    Task<Option> Undo(IGraphTrxContext graphContext);
}
