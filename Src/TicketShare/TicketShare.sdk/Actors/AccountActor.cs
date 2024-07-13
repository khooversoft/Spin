using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Actors;

public interface IAccountActor : IGrainWithStringKey
{
    Task<Option<AccountRecord>> Get(string principalId, ScopeContext context);
    Task<Option> Set(AccountRecord accountName, ScopeContext context);
}

[StatelessWorker]
public class AccountActor : Grain, IAccountActor
{
    private readonly ILogger<AccountActor> _logger;
    private readonly IClusterClient _clusterClient;

    public AccountActor(IClusterClient clusterClient, ILogger<AccountActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }


    public async Task<Option<AccountRecord>> Get(string principalId, ScopeContext context)
    {
        if (principalId.IsEmpty()) return StatusCode.BadRequest;
        context = context.With(_logger);

        string command = $"select (key={IdentityTool.ToUserKey(principalId)}) return user;";
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<AccountRecord>();

        var principalIdentity = resultOption.Return().ReturnNames.ReturnNameToObject<AccountRecord>("user");
        return principalIdentity;
    }

    public async Task<Option> Set(AccountRecord user, ScopeContext context)
    {
        context.With(_logger);
        if (user.Validate().LogStatus(context, $"UserId={user.PrincipalId}").IsError(out Option v)) return v;

        // Build graph commands
        string command = AccountRecord.Schema.Code(user).BuildSetCommands().Join(Environment.NewLine);

        var result = await _clusterClient.GetDirectoryActor().ExecuteBatch(command, context);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }
}
