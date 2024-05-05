using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Types;

namespace Toolbox.Graph;

public class InMemoryGraphFileStore : InMemoryFileStore, IGraphFileStore
{
    public InMemoryGraphFileStore(ILogger<InMemoryFileStore> logger) : base(logger) { }
}
