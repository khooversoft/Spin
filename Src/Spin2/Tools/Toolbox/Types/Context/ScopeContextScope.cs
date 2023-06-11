using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Types;

public record struct ScopeContextScope : IDisposable
{
    private readonly ScopeContextLocation _value;
    private Action<ScopeContextLocation>? _finalizeAction;
    public ScopeContextScope(ScopeContextLocation context, Action<ScopeContextLocation> action)
    {
        _value = context.NotNull();
        _value.Context.Logger.NotNull();
        _finalizeAction = action.NotNull();
    }

    public void Dispose() => Interlocked.Exchange(ref _finalizeAction, null)?.Invoke(_value);

    public static implicit operator ScopeContextLocation(ScopeContextScope scope) => scope._value;
    public static implicit operator ScopeContext(ScopeContextScope scope) => scope._value.Context;
}
