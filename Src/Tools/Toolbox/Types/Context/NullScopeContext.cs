using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Toolbox.Types;

public static class NullScopeContext
{
    public static ScopeContext Instance { get; } = new ScopeContext(NullLogger.Instance);

    public static ScopeContext ToScopeContext(this ILogger subject) => new ScopeContext(subject, CancellationToken.None);
}
