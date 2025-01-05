using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace TicketShare.sdk;

public class AuthenticationAccess
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<AuthenticationAccess> _logger;

    public AuthenticationAccess(AuthenticationStateProvider authenticationStateProvider, ILogger<AuthenticationAccess> logger)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
    }

    public async Task<bool> IsAuthenticated()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.IsAuthenticated == false;
    }

    public async Task<string?> GetPrincipalId()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        string? name = authState.User.Identity?.IsAuthenticated == true ? authState.User.Identity.Name : null;
        return name;
    }
}
