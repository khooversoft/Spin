using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public interface IIdentityActor : IGrainWithStringKey
{
    Task<Option> Delete(string id, ScopeContext context);
    Task<Option<PrincipalIdentity>> GetById(string id, ScopeContext context);
    Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context);
    Task<Option<PrincipalIdentity>> GetByUserName(string userName, ScopeContext context);
    Task<Option<PrincipalIdentity>> GetByEmail(string email, ScopeContext context);
    Task<Option> Set(PrincipalIdentity user, ScopeContext context);
}

[StatelessWorker]
public class IdentityActor : Grain, IIdentityActor
{
    private readonly ILogger<IdentityActor> _logger;
    private readonly IClusterClient _clusterClient;

    public IdentityActor(IClusterClient clusterClient, ILogger<IdentityActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string id, ScopeContext context)
    {
        context = context.With(_logger);

        string command = PrincipalIdentity.DeleteNode(id);
        Option<QueryResult> resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus();

        return resultOption.ToOptionStatus();
    }

    public async Task<Option<PrincipalIdentity>> GetById(string id, ScopeContext context)
    {
        context = context.With(_logger);

        string command = PrincipalIdentity.GetById(id);
        return await Exec(command, context);
    }

    public async Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context)
    {
        context = context.With(_logger);

        string command = PrincipalIdentity.GetByLogin(loginProvider, providerKey);
        return await Exec(command, context);
    }

    public async Task<Option<PrincipalIdentity>> GetByUserName(string userName, ScopeContext context)
    {
        context = context.With(_logger);

        string command = PrincipalIdentity.GetByUserName(userName);
        return await Exec(command, context);
    }

    public async Task<Option<PrincipalIdentity>> GetByEmail(string email, ScopeContext context)
    {
        context = context.With(_logger);

        string command = PrincipalIdentity.GetByEmail(email);
        return await Exec(command, context);
    }

    public async Task<Option> Set(PrincipalIdentity user, ScopeContext context)
    {
        context.With(_logger);
        if (user.Validate().LogStatus(context, $"UserId={user.PrincipalId}").IsError(out Option v)) return v;

        // Build graph commands
        var cmds = new Sequence<string>();

        var currentRecordOption = await GetById(user.PrincipalId, context);
        cmds += currentRecordOption.IsOk() switch
        {
            true => PrincipalIdentity.Schema.Code(user).SetCurrent(currentRecordOption.Return()).BuildSetCommands(),
            false => PrincipalIdentity.Schema.Code(user).BuildSetCommands(),
        };

        string command = cmds.Join(Environment.NewLine);
        var result = await _clusterClient.GetDirectoryActor().ExecuteBatch(command, context);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }

    private async Task<Option<PrincipalIdentity>> Exec(string command, ScopeContext context)
    {
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<PrincipalIdentity>();

        throw new NotImplementedException();

        //var principalIdentity = resultOption.Return().DataLinks.DataLinkToObject<PrincipalIdentity>("entity");
        //if (principalIdentity.IsError()) return principalIdentity.LogStatus(context, command);
        //if (!principalIdentity.Return().Validate(out var r)) return r.LogStatus(context, command).ToOptionStatus<PrincipalIdentity>();
        //return principalIdentity;
    }
}
