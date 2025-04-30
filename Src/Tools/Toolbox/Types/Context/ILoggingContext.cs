using Toolbox.Tools;

namespace Toolbox.Types;

public interface ILoggingContext
{
    public ScopeContext Context { get; }
    public (string? message, object?[] args) AppendContext(string? message, object?[] args);
}
