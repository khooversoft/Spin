using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Identity;

public static class IdentityStartup
{
    public static IServiceCollection AddIdentity(this IServiceCollection service)
    {
        service.AddSingleton<IdentityClient>();
        return service;
    }
}
