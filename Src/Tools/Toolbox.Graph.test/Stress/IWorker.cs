using Toolbox.Types;

namespace Toolbox.Graph.test.Stress;

internal interface IWorker
{
    Task<bool> Run(CancellationTokenSource token, ScopeContext context);
}
