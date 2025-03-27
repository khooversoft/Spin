using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Store;

public class InMemoryLeaseControl : IFileLeaseControl
{
    public Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, TimeSpan timeOut, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option<IFileLeasedAccess>> AcquireExclusive(TimeSpan timeOut, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option<IFileLeasedAccess>> Break(TimeSpan leaseDuration, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option> Clear(string path, ScopeContext context)
    {
        throw new NotImplementedException();
    }
}
