using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public interface IIdentityClient
{
    Task<Option> Delete(string principalId, ScopeContext context);
    Task<Option<PrincipalIdentity>> Get(string principalId, ScopeContext context);
    Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context);
    Task<Option<PrincipalIdentity>> GetByName(string normalizedUserName, ScopeContext context);
    Task<Option> Set(PrincipalIdentity user, ScopeContext context);
}

public class IdentityClient : IIdentityClient
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<IdentityClient> _logger;

    public IdentityClient(IGraphClient graphClient, ILogger<IdentityClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);

        var readOption = await Get(principalId, context);
        if (readOption.IsError()) return readOption.LogStatus(context, "Get principalId").ToOptionStatus();

        PrincipalIdentity user = readOption.Return();
        var seq = new Sequence<string>();

        seq += GraphTool.DeleteNodeCommand(PrincipalIdentityTool.ToUserKey(user.PrincipalId));
        seq += GraphTool.DeleteNodeCommand(PrincipalIdentityTool.ToEmailIndex(user.Email));
        seq += GraphTool.DeleteNodeCommand(PrincipalIdentityTool.ToUserNameIndex(user.NormalizedUserName));

        if ((user.LoginProvider.IsNotEmpty() && user.ProviderKey.IsNotEmpty()))
        {
            seq += GraphTool.DeleteNodeCommand(PrincipalIdentityTool.ToLoginIndex(user.LoginProvider, user.ProviderKey));
        }

        string cmds = seq.Join(Environment.NewLine);

        var result = await _graphClient.ExecuteBatch(cmds, context);
        result.LogStatus(context, $"Deleting principalId={principalId}");
        return result.ToOptionStatus();
    }

    public async Task<Option<PrincipalIdentity>> Get(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);

        var cmd = $"select (key={PrincipalIdentityTool.ToUserKey(principalId)}) return entity ;";
        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to find principalId={principalId}", principalId);
            return result.LogStatus(context, $"principalId={principalId}").ToOptionStatus<PrincipalIdentity>();
        }

        return result.Return().DataLinkToObject<PrincipalIdentity>("entity");
    }

    public async Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context)
    {
        loginProvider.NotEmpty();
        providerKey.NotEmpty();
        context = context.With(_logger);

        var cmd = $"select (key={PrincipalIdentityTool.ToLoginIndex(loginProvider, providerKey)}) -> [*] -> (*) return entity ;";
        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to find login, loginProvider={loginProvider},providerKey={providerKey}", loginProvider, providerKey);
            return result.LogStatus(context, $"login={loginProvider}").ToOptionStatus<PrincipalIdentity>();
        }

        return result.Return().DataLinkToObject<PrincipalIdentity>("entity");
    }

    public async Task<Option<PrincipalIdentity>> GetByName(string normalizedUserName, ScopeContext context)
    {
        normalizedUserName.NotEmpty();
        context = context.With(_logger);

        var cmd = $"select (key={PrincipalIdentityTool.ToUserNameIndex(normalizedUserName)}) -> [*] -> (*) return entity ;";
        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to find by userName={userName}", normalizedUserName);
            return result.LogStatus(context, $"userName={normalizedUserName}").ToOptionStatus<PrincipalIdentity>();
        }

        return result.Return().DataLinkToObject<PrincipalIdentity>("entity");
    }

    public async Task<Option> Set(PrincipalIdentity user, ScopeContext context)
    {
        context = context.With(_logger);
        if (!user.Validate(out var r)) return r.LogStatus(context, nameof(PrincipalIdentity));

        string nodeKey = PrincipalIdentityTool.ToUserKey(user.PrincipalId);
        string userNameNodeKey = PrincipalIdentityTool.ToUserNameIndex(user.NormalizedUserName);
        string emailNodeKey = PrincipalIdentityTool.ToEmailIndex(user.Email);

        string emailTag = $"email={emailNodeKey}";
        string userNameNameTag = $"userName={userNameNodeKey}";

        string? loginProviderKey = user.HasLoginProvider() ? PrincipalIdentityTool.ToLoginIndex(user.LoginProvider!, user.ProviderKey!) : null;
        string? loginProviderTag = user.HasLoginProvider() ? $"loginProvider={loginProviderKey}" : "-loginProvider";

        var seq = new Sequence<string>();
        seq += await GetChangeCommands(user, context);

        string tags = new[] { emailTag, userNameNameTag, loginProviderTag }.Where(x => x != null).Join(',');
        seq += GraphTool.SetNodeCommand(nodeKey, tags, user.ToJson().ToBase64(), "entity");

        seq += GraphTool.SetNodeCommand(userNameNodeKey);
        seq += GraphTool.SetEdgeCommands(userNameNodeKey, nodeKey, GraphConstants.UniqueIndexEdgeType);

        seq += GraphTool.SetNodeCommand(emailNodeKey);
        seq += GraphTool.SetEdgeCommands(emailNodeKey, nodeKey, GraphConstants.UniqueIndexEdgeType);

        if (loginProviderKey.IsNotEmpty())
        {
            seq += GraphTool.SetNodeCommand(loginProviderKey);
            seq += GraphTool.SetEdgeCommands(loginProviderKey, nodeKey, GraphConstants.UniqueIndexEdgeType);
        }

        var cmds = seq.Join(Environment.NewLine);

        var result = await _graphClient.Execute(cmds, context);
        return result.ToOptionStatus();
    }

    private async Task<IEnumerable<string>> GetChangeCommands(PrincipalIdentity user, ScopeContext context)
    {
        var seq = new Sequence<string>();

        var currentRecordOption = await Get(user.PrincipalId, context);
        if (currentRecordOption.IsError()) return seq;

        PrincipalIdentity current = currentRecordOption.Return();

        if (current.Email != user.Email)
        {
            seq += GraphTool.DeleteNodeCommand(PrincipalIdentityTool.ToEmailIndex(current.Email));
        }

        if (current.NormalizedUserName != user.NormalizedUserName)
        {
            seq += GraphTool.DeleteNodeCommand(PrincipalIdentityTool.ToUserNameIndex(current.NormalizedUserName));
        }

        return seq;
    }
}
