using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

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
        return await _graphClient.DeleteNode(IdentityTool.ToNodeKey(principalId), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByPrincipalId(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        return await _graphClient.GetNode<PrincipalIdentity>(IdentityTool.ToNodeKey(principalId), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context)
    {
        loginProvider.NotEmpty();
        providerKey.NotEmpty();

        return await _graphClient.GetByTag<PrincipalIdentity>(IdentityTool.ConstructLoginProviderTag(loginProvider, providerKey).NotEmpty(), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByEmail(string email, ScopeContext context)
    {
        email.NotEmpty();
        return await _graphClient.GetByTag<PrincipalIdentity>(IdentityTool.ConstructEmailTag(email), context).ConfigureAwait(false);
    }

    public async Task<Option<PrincipalIdentity>> GetByName(string normalizedUserName, ScopeContext context)
    {
        normalizedUserName.NotEmpty();
        return await _graphClient.GetByTag<PrincipalIdentity>(IdentityTool.ConstructUserNameTag(normalizedUserName), context).ConfigureAwait(false);
    }

    public async Task<Option> Set(PrincipalIdentity user, ScopeContext context)
    {
        context = context.With(_logger);
        if (user.Validate().IsError(out var r)) return r.LogStatus(context, nameof(PrincipalIdentity));

        string nodeKey = IdentityTool.ToNodeKey(user.PrincipalId);

        string emailTag = IdentityTool.ConstructEmailTag(user.Email);
        string userNameNameTag = IdentityTool.ConstructUserNameTag(user.UserName.ToNullIfEmpty() ?? user.NormalizedUserName);
        string loginProviderTag = IdentityTool.ConstructLoginProviderTag(user.LoginProvider, user.ProviderKey) ?? "-loginProvider";

        var cmd = new NodeCommandBuilder()
            .UseSet()
            .SetNodeKey(nodeKey)
            .AddTag(IdentityTool.NodeTag)
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
}
