using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Tools;

public class FinalizeScope : IDisposable
{
    private Action? _action;

    public FinalizeScope(Action action) => _action = action.NotNull();

    public void Dispose() => Interlocked.Exchange(ref _action, null)?.Invoke();
}
