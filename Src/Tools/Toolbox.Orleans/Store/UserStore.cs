using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public class UserStore : IUserStore<PrincipalIdentity>, IUserLoginStore<PrincipalIdentity>
{
    private readonly ILogger<UserStore> _logger;
    private readonly IClusterClient _clusterClient;

    public UserStore(IClusterClient clusterClient, ILogger<UserStore> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task AddLoginAsync(PrincipalIdentity user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        var context = new ScopeContext(_logger);
        IIdentityActor identityActor = _clusterClient.GetIdentityActor();
        
        user.LoginProvider = login.LoginProvider;
        user.ProviderKey = login.ProviderKey;
        user.ProviderDisplayName = login.ProviderDisplayName;

        var identityResult = (await identityActor.Set(user, context)).ToIdentityResult();
    }

    public async Task<IdentityResult> CreateAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();
        var context = new ScopeContext(_logger);
        IIdentityActor identityActor = _clusterClient.GetIdentityActor();

        var identityResult = (await identityActor.Set(user, context)).ToIdentityResult();
        return identityResult;
    }

    public async Task<IdentityResult> DeleteAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();
        var context = new ScopeContext(_logger);
        IIdentityActor identityActor = _clusterClient.GetIdentityActor();

        var identityResult = (await identityActor.Delete(user.Id, context)).ToIdentityResult();
        return identityResult;
    }

    public void Dispose() { }

    public async Task<PrincipalIdentity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        userId.NotEmpty();
        var context = new ScopeContext(_logger);
        IIdentityActor identityActor = _clusterClient.GetIdentityActor();

        var result = await identityActor.GetById(userId, context);
        return result.IsOk() ? result.Return() : null;
    }

    public async Task<PrincipalIdentity?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        loginProvider.NotEmpty();
        providerKey.NotEmpty();
        var context = new ScopeContext(_logger);
        IIdentityActor identityActor = _clusterClient.GetIdentityActor();

        var result = await identityActor.GetByLogin(loginProvider, providerKey, context);
        return result.IsOk() ? result.Return() : null;
    }

    public async Task<PrincipalIdentity?> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        userName.NotEmpty();
        var context = new ScopeContext(_logger);
        IIdentityActor identityActor = _clusterClient.GetIdentityActor();

        var result = await identityActor.GetByUserName(userName, context);
        return result.IsOk() ? result.Return() : null;
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetNormalizedUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken) =>
        user.NormalizedUserName.ToTaskResult<string?>();

    public Task<string> GetUserIdAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();

        return user.Id.ToTaskResult();
    }

    public Task<string?> GetUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();

        return user.UserName.ToTaskResult<string?>();
    }

    public Task RemoveLoginAsync(PrincipalIdentity user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetNormalizedUserNameAsync(PrincipalIdentity user, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();
        normalizedName.NotEmpty();

        user.Id = normalizedName;
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(PrincipalIdentity user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();
        userName.NotEmpty();

        user.UserName = userName;
        user.Email = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();
        var context = new ScopeContext(_logger);
        IIdentityActor identityActor = _clusterClient.GetIdentityActor();

        var identityResult = (await identityActor.Set(user, context)).ToIdentityResult();
        return identityResult;
    }
}
