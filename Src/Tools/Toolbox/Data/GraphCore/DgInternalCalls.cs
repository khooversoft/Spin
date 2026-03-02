using Toolbox.Types;

namespace Toolbox.Data;

public class DgInternalCalls
{
    public Func<TrxSourceRecorder?> GetRecorder { get; set; } = null!;
    public Func<string, bool> IsNodeExist { get; set; } = null!;
    public Func<string, Option> NodeDeleted { get; set; } = null!;
    public Action ClearNodes { get; set; } = null!;
    public Action ClearEdges { get; set; } = null!;
}
