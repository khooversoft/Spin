using Toolbox.Types;

namespace Toolbox.Graph;

public interface IChangeLog
{
    Guid LogKey { get; }
    Option Undo(GraphChangeContext graphContext);
}
