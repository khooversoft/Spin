using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public class IdentityUserStore : IUserStore<PrincipalIdentity>
{
    private readonly IdentityClient identityClient;
    private readonly ILogger<IdentityUserStore> _logger;

    public IdentityUserStore(IdentityClient clusterClient, ILogger<IdentityUserStore> logger)
    {
        identityClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<IdentityResult> CreateAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var identityResult = (await identityClient.Set(user, context)).ToIdentityResult();
        return identityResult;
    }

    public async Task<IdentityResult> DeleteAsync(PrincipalIdentity user, CancellationToken cancellationToken = default)
    {
        user.NotNull();
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var identityResult = (await identityClient.Delete(user.PrincipalId, context)).ToIdentityResult();
        return identityResult;
    }

    public void Dispose() { }

    public async Task<PrincipalIdentity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var result = await identityClient.GetByPrincipalId(userId, context);
        return result.IsOk() ? result.Return() : null;
    }

    public async Task<PrincipalIdentity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var result = await identityClient.GetByName(normalizedUserName, context);
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

    public async Task SetNormalizedUserNameAsync(PrincipalIdentity user, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();
        normalizedName.NotEmpty();

        user.NormalizedUserName = normalizedName;
        await UpdateAsync(user, cancellationToken);
    }

    public async Task SetUserNameAsync(PrincipalIdentity user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NotNull();

        user.UserName = userName.NotEmpty();
        await UpdateAsync(user, cancellationToken);
    }

    public async Task<IdentityResult> UpdateAsync(PrincipalIdentity user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ScopeContext(_logger);

        var identityResult = (await identityClient.Set(user, context)).ToIdentityResult();
        return identityResult;
    }
}
