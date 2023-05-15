using Toolbox.DocumentContainer;
using Toolbox.Types;

namespace Toolbox.Block.Contract;

public interface IContract
{
    DocumentId DocumentId { get; }
    Task<StatusCode> Start(ScopeContext context);
    Task<StatusCode> Stop(ScopeContext context);
}
