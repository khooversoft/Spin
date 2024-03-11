using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Tools;

namespace Toolbox.Identity;

public static class Startup
{
    public static ISiloBuilder AddIdentityActor(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IdentityService>();
            services.AddSingleton<IIdentityClient, IdentityActorConnector>();
        });

        return builder;
    }
}