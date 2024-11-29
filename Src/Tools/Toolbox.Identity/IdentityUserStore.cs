using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public class IdentityUserStore : IUserStore<PrincipalIdentity>, IUserLoginStore<PrincipalIdentity>
{
    private readonly IdentityClient _identityClient;
    private readonly ILogger<IdentityUserStore> _logger;

    public IdentityUserStore(IdentityClient clusterClient, ILogger<IdentityUserStore> logger)
    {
        _identityClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    #region IUserStore

    public async Task<IdentityResult> CreateAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var identityResult = (await _identityClient.Set(user, context)).ToIdentityResult();
        return identityResult;
    }

    public async Task<IdentityResult> DeleteAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
    {
        user.NotNull();
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var identityResult = (await _identityClient.Delete(user.PrincipalId, context)).ToIdentityResult();
        return identityResult;
    }

    public void Dispose() { }

    public async Task<PrincipalIdentity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var result = await _identityClient.GetByPrincipalId(userId, context);
        return result.IsOk() ? result.Return() : null;
    }

    public async Task<PrincipalIdentity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var result = await _identityClient.GetByName(normalizedUserName, context);
        return result.IsOk() ? result.Return() : null;
    }

    public Task<string?> GetNormalizedUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();

        return user.NormalizedUserName.ToTaskResult<string?>();
    }

    public Task<string> GetUserIdAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();

        return user.PrincipalId.NotEmpty().ToTaskResult();
    }

    public Task<string?> GetUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();

        return user.UserName.ToTaskResult<string?>();
    }

    public Task SetNormalizedUserNameAsync(PrincipalIdentity user, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();
        normalizedName.NotEmpty();

        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(PrincipalIdentity user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();

        user.UserName = userName.NotEmpty();
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var identityResult = (await _identityClient.Set(user, context)).ToIdentityResult();
        return identityResult;
    }

    #endregion

    #region IUserLoginStore

    public Task AddLoginAsync(PrincipalIdentity user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.LoginProvider = login.LoginProvider;
        user.ProviderKey = login.ProviderKey;
        user.ProviderDisplayName = login.ProviderDisplayName;

        return Task.CompletedTask;
    }

    public async Task<PrincipalIdentity?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var result = await _identityClient.GetByLogin(loginProvider, providerKey, context);
        return result.IsOk() ? result.Return() : null;
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var resultOption = await _identityClient.GetByPrincipalId(user.PrincipalId, context);
        if (resultOption.IsError()) return Array.Empty<UserLoginInfo>();

        var result = resultOption.Return();
        if (result.LoginProvider.IsEmpty() || result.ProviderKey.IsEmpty() || result.ProviderDisplayName.IsEmpty()) return Array.Empty<UserLoginInfo>();

        var info = new UserLoginInfo(result.LoginProvider, result.ProviderKey, result.ProviderDisplayName);
        return [info];
    }

    public async Task RemoveLoginAsync(PrincipalIdentity user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var result = await _identityClient.Delete(user.PrincipalId, context);
        result.LogStatus(context, nameof(RemoveLoginAsync)).ThrowOnError();
    }

    #endregion
}
