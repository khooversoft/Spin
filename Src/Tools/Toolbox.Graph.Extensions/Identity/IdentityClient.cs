using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public class IdentityClient
{
    private readonly IGraphClient _graphClient;
    public IdentityClient(IGraphClient graphClient) => _graphClient = graphClient.NotNull();

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
        var queryOption = user.CreateQuery(context);
        if (user.Validate().IsError(out var r)) return r.LogStatus(context, nameof(PrincipalIdentity));

        string cmd = queryOption.Return();
        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        result.LogStatus(context, "Set principal user, principalId={principalId}", [user.PrincipalId]);

        return result.ToOptionStatus();
    }
}
