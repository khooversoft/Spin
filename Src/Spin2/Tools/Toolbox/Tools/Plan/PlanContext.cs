using Toolbox.Types;

namespace Toolbox.Tools;

public class PlanContext
{
    public PlanMode Mode { get; init; }
    public IServiceProvider Service { get; init; } = null!;
    public IDictionary<string, object?> States { get; init; } = new Dictionary<string, object?>();
    public IList<Option> History { get; init; } = new List<Option>();
}
