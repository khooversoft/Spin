using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Identity;

public static class IdentityStartup
{
    public static IServiceCollection AddToolboxIdentity(this IServiceCollection service)
    {
        service.AddSingleton<IdentityClient>();
        service.AddSingleton<IUserStore<PrincipalIdentity>, IdentityUserStore>();
        return service;
    }
}
