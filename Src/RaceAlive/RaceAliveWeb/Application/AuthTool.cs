using System.Security.Claims;

namespace RaceAliveWeb.Application;

public static class AuthTool
{
    public static AuthUser? GetAuthUser(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true) return null;

        var displayName = user.FindFirst("name")?.Value
           ?? user.FindFirst("preferred_username")?.Value
           ?? user.Identity?.Name;

        var email = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
            ?? user.FindFirst("email")?.Value
            ?? user.FindFirst("preferred_username")?.Value
            ?? user.Identity?.Name;

        var authUser = (displayName, email) switch
        {
            (null, null) => new AuthUser { DisplayName = "<unknown>" },
            (string v1, null) => new AuthUser { DisplayName = v1, },
            (null, string v2) => new AuthUser { DisplayName = v2, Email = v2 },
            (string v1, string v2) => new AuthUser { DisplayName = v1, Email = v2 },
        };

        return authUser;
    }
}

public record AuthUser
{
    public string DisplayName { get; init; } = null!;
    public string? Email { get; init; }

    public string GetDisplayName()
    {
        return Email switch
        {
            null => DisplayName,
            _ => $"{DisplayName} ({Email})"
        };
    }
}
