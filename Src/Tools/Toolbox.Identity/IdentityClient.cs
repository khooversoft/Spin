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

        var cmd = new DeleteCommandBuilder()
            .SetIfExist()
            .SetNodeKey(PrincipalIdentityTool.ToUserKey(principalId))
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to delete principalId={principalId}", principalId);
            return result.LogStatus(context, $"principalId={principalId}").ToOptionStatus();
        }

        result.LogStatus(context, $"Deleting principalId={principalId}");
        return result.ToOptionStatus();
    }

    public async Task<Option<PrincipalIdentity>> Get(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);

        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey(PrincipalIdentityTool.ToUserKey(principalId)))
            .AddDataName("entity")
            .Build();

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

        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.AddTag(ConstructLoginProviderTag(loginProvider, providerKey).NotEmpty()))
            .AddDataName("entity")
            .Build();

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

        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.AddTag(ConstructUserNameTag(normalizedUserName)))
            .AddDataName("entity")
            .Build();

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

        string emailTag = ConstructEmailTag(user.Email);
        string userNameNameTag = ConstructUserNameTag(user.NormalizedUserName);
        string loginProviderTag = ConstructLoginProviderTag(user.LoginProvider, user.ProviderKey) ?? "-loginProvider";

        var cmd = new NodeCommandBuilder()
            .UseSet()
            .SetNodeKey(nodeKey)
            .AddTag(emailTag)
            .AddTag(userNameNameTag)
            .AddTag(loginProviderTag)
            .AddData("entity", user)
            .AddIndex("email")
            .AddIndex("userName")
            .Action(x =>
            {
                if (user.HasLoginProvider()) x.AddIndex("loginProvider");
            })
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
        }
        return result.ToOptionStatus();
    }

    private static string ConstructEmailTag(string value) => $"email={value.ToLower()}";
    private static string ConstructUserNameTag(string value) => $"userName={value.ToLower()}";
    private static string? ConstructLoginProviderTag(string? loginProvider, string? providerKey)
    {
        return (loginProvider.IsNotEmpty() && providerKey.IsNotEmpty()) switch
        {
            false => null,
            true => $"loginProvider={loginProvider.ToLower()}/{providerKey.ToLower()}",
        };
    }
}
