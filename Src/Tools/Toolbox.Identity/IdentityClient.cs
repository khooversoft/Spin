using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;


public class IdentityClient
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
        return await _graphClient.DeleteNode(ToUserKey(principalId), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByPrincipalId(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        return await _graphClient.GetNode<PrincipalIdentity>(ToUserKey(principalId), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context)
    {
        loginProvider.NotEmpty();
        providerKey.NotEmpty();

        return await _graphClient.GetByTag<PrincipalIdentity>(ConstructLoginProviderTag(loginProvider, providerKey).NotEmpty(), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByEmail(string email, ScopeContext context)
    {
        email.NotEmpty();
        return await _graphClient.GetByTag<PrincipalIdentity>(ConstructEmailTag(email), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByName(string normalizedUserName, ScopeContext context)
    {
        normalizedUserName.NotEmpty();
        return await _graphClient.GetByTag<PrincipalIdentity>(ConstructUserNameTag(normalizedUserName), context).ConfigureAwait(false);
    }

    public async Task<Option> Set(PrincipalIdentity user, ScopeContext context)
    {
        context = context.With(_logger);
        if (!user.Validate(out var r)) return r.LogStatus(context, nameof(PrincipalIdentity));

        string nodeKey = ToUserKey(user.PrincipalId);

        string emailTag = ConstructEmailTag(user.Email);
        string userNameNameTag = ConstructUserNameTag(user.NormalizedUserName);
        string loginProviderTag = ConstructLoginProviderTag(user.LoginProvider, user.ProviderKey) ?? "-loginProvider";

        var cmd = new NodeCommandBuilder()
            .UseSet()
            .SetNodeKey(nodeKey)
            .AddTag(NodeTypeTag)
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

        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        result.LogStatus(context, "Set principal user, nodeKey={nodeKey}", [nodeKey]);

        return result.ToOptionStatus();
    }

    public static string ToUserKey(string id) => $"user:{id.NotEmpty().ToLower()}";
    public static string NodeTypeTag { get; } = nameof(PrincipalIdentity).ToLowerInvariant();
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
