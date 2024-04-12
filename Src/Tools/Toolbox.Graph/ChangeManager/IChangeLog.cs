using Toolbox.Types;

namespace Toolbox.Graph;

public interface IChangeLog
{
    Guid LogKey { get; }
    Option Undo(GraphContext graphContext);
    ChangeTrx GetChangeTrx(Guid trxKey);
    ChangeTrx GetUndoChangeTrx(Guid trxKey);
}
