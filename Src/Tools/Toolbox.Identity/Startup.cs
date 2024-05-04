using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Tools;

namespace Toolbox.Identity;

public static class Startup
{
    public static ISiloBuilder AddIdentityActor(this ISiloBuilder builder, string resourceId = "directory")
    {
        builder.NotNull();
        resourceId = resourceId.NotNull().ToLower();

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IdentityService>();
            //services.AddSingleton<IIdentityClient>(service =>
            //{
            //    return ActivatorUtilities.CreateInstance<IdentityActorConnector>(service, resourceId);
            //});
        });

        return builder;
    }
}