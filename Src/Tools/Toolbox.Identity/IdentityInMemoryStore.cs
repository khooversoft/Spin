using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public class IdentityInMemoryStore : IIdentityClient
{
    private readonly ILogger _logger;
    private readonly GraphMap _map = new GraphMap();

    public IdentityInMemoryStore(ILogger<IdentityInMemoryStore> logger) => _logger = logger.NotNull();

    public Task<Option> Clear(string principalId, string traceId)
    {
        _map.Clear();
        return ((Option)StatusCode.OK).ToTaskResult();
    }

    public Task<Option<GraphQueryResults>> Execute(string command, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        return _map.Execute(command).ToTaskResult();
    }
}
