using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TicketShare.sdk.Actors;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class AccountConnector
{
    private readonly ILogger<AccountConnector> _logger;
    private readonly IClusterClient _clusterClient;

    public AccountConnector(IClusterClient clusterClient, ILogger<AccountConnector> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<AccountRecord>> Get(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);
        IAccountActor userActor = _clusterClient.GetUserActor();

        return await userActor.Get(principalId, context);
    }

    public async Task<Option> Set(AccountRecord accountRecord, ScopeContext context)
    {
        if( !accountRecord.Validate(out var r)) return r; 
        context = context.With(_logger);
        IAccountActor userActor = _clusterClient.GetUserActor();

        return await userActor.Set(accountRecord, context);
    }
}