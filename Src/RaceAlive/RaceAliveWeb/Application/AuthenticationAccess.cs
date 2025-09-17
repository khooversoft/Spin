using Microsoft.AspNetCore.Components.Authorization;
using Toolbox.Tools;

namespace RaceAliveWeb.Application;

public class AuthenticationAccess
{
    private readonly AuthenticationStateProvider _authProvider;

    public AuthenticationAccess(AuthenticationStateProvider authProvider) => _authProvider = authProvider.NotNull();

    public async Task<string> GetDisplayName()
    {
        var authUser = (await _authProvider.GetAuthenticationStateAsync()).User.GetAuthUser();
        return authUser?.GetDisplayName() ?? "Unknown user";
    }

    public async Task<string> GetUserName()
    {
        var authUser = (await _authProvider.GetAuthenticationStateAsync()).User.GetAuthUser();
        return authUser?.DisplayName ?? "Unknown user";
    }

    public async Task<string?> GetEmail()
    {
        var authUser = (await _authProvider.GetAuthenticationStateAsync()).User.GetAuthUser();
        return authUser?.Email;
    }
}
