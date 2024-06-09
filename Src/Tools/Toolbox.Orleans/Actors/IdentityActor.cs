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

        string command = $"delete (key={ToUserKey(id)});";
        Option<GraphQueryResult> resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus();

        return resultOption.ToOptionStatus();
    }

    public async Task<Option<PrincipalIdentity>> GetById(string id, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select (key={ToUserKey(id)}) return entity;";
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<PrincipalIdentity>();

        var principalIdentity = resultOption.Return().ReturnNames.ReturnNameToObject<PrincipalIdentity>("entity");
        return principalIdentity;
    }

    public async Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select (key={ToLoginIndex(loginProvider, providerKey)}) -> [*] -> (*) return entity;";
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<PrincipalIdentity>();

        var principalIdentity = resultOption.Return().ReturnNames.ReturnNameToObject<PrincipalIdentity>("entity");
        return principalIdentity;
    }

    public async Task<Option<PrincipalIdentity>> GetByUserName(string userName, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select (key={ToUserNameIndex(userName)}) -> [*] -> (*) return entity;";
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<PrincipalIdentity>();

        var principalIdentity = resultOption.Return().ReturnNames.NotNull().ReturnNameToObject<PrincipalIdentity>("entity");
        return principalIdentity;
    }

    public async Task<Option<PrincipalIdentity>> GetByEmail(string email, ScopeContext context)
    {
        context = context.With(_logger);

        string command = $"select (key={ToEmailIndex(email)}) -> [*] -> (*) return entity;";
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<PrincipalIdentity>();

        var principalIdentity = resultOption.Return().ReturnNames.ReturnNameToObject<PrincipalIdentity>("entity");
        if (principalIdentity.IsError()) return principalIdentity.LogStatus(context, command);

        return principalIdentity;
    }

    public async Task<Option> Set(PrincipalIdentity user, ScopeContext context)
    {
        context.With(_logger);
        if (user.Validate().LogStatus(context, $"UserId={user.Id}").IsError(out Option v)) return v;
        var directoryActor = _clusterClient.GetDirectoryActor();

        // Build graph commands
        var cmds = new Sequence<string>();
        string base64 = user.ToJson64();
        string? tags = user.Email.IsNotEmpty() ? $"email={user.Email}" : null;

        cmds += GraphTool.CreateNodeCommand(ToUserKey(user.Id), tags, base64);

        // User Name node -> user node
        if (user.UserName.IsNotEmpty()) cmds += GraphTool.CreateIndexCommands(ToUserNameIndex(user.UserName), ToUserKey(user.Id));

        // Email node -> user node
        if (user.Email.IsNotEmpty()) cmds += GraphTool.CreateIndexCommands(ToEmailIndex(user.Email), ToUserKey(user.Id));

        // Login node -> user node
        if (user.LoginProvider.IsNotEmpty() && user.ProviderKey.IsNotEmpty()) cmds += GraphTool.CreateIndexCommands(ToLoginIndex(user.LoginProvider, user.ProviderKey), ToUserKey(user.Id));

        string command = cmds.Join(Environment.NewLine);
        var result = await directoryActor.ExecuteBatch(command, context);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }

    private static string ToUserKey(string id) => $"user:{id.NotEmpty().ToLower()}";
    private static string ToUserNameIndex(string userName) => $"userName:{userName.NotEmpty().ToLower()}";
    private static string ToEmailIndex(string userName) => $"userEmail:{userName.NotEmpty().ToLower()}";
    private static string ToLoginIndex(string provider, string providerKey) => $"logonProvider:{provider.NotEmpty().ToLower() + "/" + providerKey.NotEmpty().ToLower()}";
}
