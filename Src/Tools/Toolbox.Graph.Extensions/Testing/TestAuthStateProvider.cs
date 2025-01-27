using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Toolbox.Tools;

namespace Toolbox.Graph.Extensions.Testing;

public class TestAuthStateProvider : AuthenticationStateProvider
{
    private readonly string _principalId = "user1@domain.com";

    public TestAuthStateProvider() { }
    public TestAuthStateProvider(string principalId) => _principalId = principalId.NotEmpty();

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, _principalId),
            new Claim(ClaimTypes.Role, "Administrator")
        };
        var anonymous = new ClaimsIdentity(claims, "testAuthType");

        return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
    }
}