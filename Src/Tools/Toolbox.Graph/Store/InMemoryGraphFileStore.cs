using Microsoft.Extensions.Logging;
using Toolbox.Store;

namespace Toolbox.Graph;

public class InMemoryGraphFileStore : InMemoryFileStore, IGraphFileStore
{
    public InMemoryGraphFileStore(ILogger<InMemoryFileStore> logger) : base(logger) { }
}
