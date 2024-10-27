//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Identity;

//public class UserStore : IUserStore<PrincipalIdentity>, IUserLoginStore<PrincipalIdentity>
//{
//    private readonly ILogger<UserStore> _logger;
//    private readonly IIdentityClient identityClient;

//    public UserStore(IIdentityClient clusterClient, ILogger<UserStore> logger)
//    {
//        _logger = logger.NotNull();
//        identityClient = clusterClient.NotNull();
//    }

//    #region IUserStore<T>

//    public async Task<IdentityResult> CreateAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var context = new ScopeContext(_logger);

//        var identityResult = (await identityClient.Set(user, context)).ToIdentityResult();
//        return identityResult;
//    }

//    public async Task<IdentityResult> DeleteAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
//    {
//        user.NotNull();
//        cancellationToken.ThrowIfCancellationRequested();
//        var context = new ScopeContext(_logger);

//        var identityResult = (await identityClient.Delete(user.PrincipalId, context)).ToIdentityResult();
//        return identityResult;
//    }

//    public void Dispose() { }

//    public async Task<PrincipalIdentity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var context = new ScopeContext(_logger);

//        var result = await identityClient.GetByPrincipalId(userId, context);
//        return result.IsOk() ? result.Return() : null;
//    }

//    public async Task<PrincipalIdentity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var context = new ScopeContext(_logger);

//        var result = await identityClient.GetByName(normalizedUserName, context);
//        return result.IsOk() ? result.Return() : null;
//    }

//    public Task<string?> GetNormalizedUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken) =>
//        user.NormalizedUserName.ToTaskResult<string?>();

//    public Task<string> GetUserIdAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();

//        return user.PrincipalId.NotEmpty().ToTaskResult();
//    }

//    public Task<string?> GetUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();

//        return user.UserName.ToTaskResult<string?>();
//    }


//    public Task SetNormalizedUserNameAsync(PrincipalIdentity user, string? normalizedName, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();
//        normalizedName.NotEmpty();

//        user.NormalizedUserName = normalizedName;
//        return Task.CompletedTask;
//    }

//    public Task SetUserNameAsync(PrincipalIdentity user, string? userName, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();
//        userName.NotEmpty();

//        user.UserName = userName;
//        user.Email = userName;
//        return Task.CompletedTask;
//    }

//    public async Task<IdentityResult> UpdateAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var context = new ScopeContext(_logger);

//        var identityResult = (await identityClient.Set(user, context)).ToIdentityResult();
//        return identityResult;
//    }

//    #endregion

//    #region IUserLoginStore<T>

//    public async Task AddLoginAsync(PrincipalIdentity user, UserLoginInfo login, CancellationToken cancellationToken)
//    {
//        var context = new ScopeContext(_logger);

//        user.LoginProvider = login.LoginProvider;
//        user.ProviderKey = login.ProviderKey;
//        user.ProviderDisplayName = login.ProviderDisplayName;

//        (await identityClient.Set(user, context)).ThrowOnError();
//    }

//    public Task RemoveLoginAsync(PrincipalIdentity user, string loginProvider, string providerKey, CancellationToken cancellationToken)
//    {
//        if (user.HasLoginProvider() && (user.LoginProvider != loginProvider || user.ProviderKey != providerKey))
//        {
//            user.LoginProvider = null;
//            user.ProviderKey = null;
//            user.ProviderDisplayName = null;
//        }

//        return Task.CompletedTask;
//    }

//    public Task<IList<UserLoginInfo>> GetLoginsAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        IList<UserLoginInfo> result = user.HasLoginProvider() switch
//        {
//            false => [],
//            true => [new UserLoginInfo(user.LoginProvider!, user.ProviderKey!, user.ProviderDisplayName)],
//        };

//        return result.ToTaskResult();
//    }

//    public async Task<PrincipalIdentity?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var context = new ScopeContext(_logger);

//        var result = await identityClient.GetByLogin(loginProvider, providerKey, context);
//        return result.IsOk() ? result.Return() : null;
//    }

//    #endregion
//}
