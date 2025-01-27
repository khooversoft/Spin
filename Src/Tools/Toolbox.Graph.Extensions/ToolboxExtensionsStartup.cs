using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Graph.Extensions;

public static class ToolboxExtensionsStartup
{
    public static IServiceCollection AddToolboxIdentity(this IServiceCollection service)
    {
        service.AddSingleton<IdentityClient>();
        service.AddSingleton<SecurityGroupClient>();
        service.AddSingleton<ChannelClient>();
        service.AddSingleton<IUserStore<PrincipalIdentity>, IdentityUserStore>();
        return service;
    }
}
