﻿//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Identity.Store;


//public class UserStore : IUserStore<PrincipalIdentity>, IUserLoginStore<PrincipalIdentity>
//{
//    private readonly IdentityService _identityService;
//    private readonly ILogger<UserStore> _logger;

//    public UserStore(IdentityService identityService, ILogger<UserStore> logger)
//    {
//        _identityService = identityService.NotNull();
//        _logger = logger.NotNull();
//    }

//    public Task AddLoginAsync(PrincipalIdentity user, UserLoginInfo login, CancellationToken cancellationToken)
//    {
//        user.LoginProvider = login.LoginProvider;
//        user.ProviderKey = login.ProviderKey;
//        user.ProviderDisplayName = login.ProviderDisplayName;

//        return Task.CompletedTask;
//    }

//    public async Task<IdentityResult> CreateAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();
//        var context = new ScopeContext(_logger);

//        var identityResult = (await _identityService.Set(user, null, context)).ToIdentityResult();
//        return identityResult;
//    }

//    public async Task<IdentityResult> DeleteAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();
//        var context = new ScopeContext(_logger);

//        var identityResult = (await _identityService.Delete(user.Id, context)).ToIdentityResult();
//        return identityResult;
//    }

//    public void Dispose() { }

//    public async Task<PrincipalIdentity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        userId.NotEmpty();
//        var context = new ScopeContext(_logger);

//        var result = await _identityService.GetById(userId, context);
//        return result.IsOk() ? result.Return() : null;
//    }

//    public async Task<PrincipalIdentity?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        loginProvider.NotEmpty();
//        providerKey.NotEmpty();
//        var context = new ScopeContext(_logger);

//        var result = await _identityService.FindByLogin(loginProvider, providerKey, context);
//        return result.IsOk() ? result.Return() : null;
//    }

//    public async Task<PrincipalIdentity?> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        userName.NotEmpty();
//        var context = new ScopeContext(_logger);

//        var result = await _identityService.GetByUserName(userName, context);
//        return result.IsOk() ? result.Return() : null;
//    }

//    public Task<IList<UserLoginInfo>> GetLoginsAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<string?> GetNormalizedUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken) => user.NormalizedUserName.ToTaskResult<string?>();


//    public Task<string> GetUserIdAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();

//        return user.Id.ToTaskResult();
//    }

//    public Task<string?> GetUserNameAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        user.NotNull();

//        return user.UserName.ToTaskResult<string?>();
//    }

//    public Task RemoveLoginAsync(PrincipalIdentity user, string loginProvider, string providerKey, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
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
//        return Task.CompletedTask;
//    }

//    public Task<IdentityResult> UpdateAsync(PrincipalIdentity user, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }
//}
