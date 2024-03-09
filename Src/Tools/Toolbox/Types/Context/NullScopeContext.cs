using Microsoft.Extensions.Logging.Abstractions;

namespace Toolbox.Types;

public static class NullScopeContext
{
    public static ScopeContext Instance { get; } = new ScopeContext(NullLogger.Instance);
}
