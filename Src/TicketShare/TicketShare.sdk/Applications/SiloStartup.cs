using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public static class SiloStartup
{
    public static ISiloBuilder AddBlogCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        StorageOption option = hostContext.Configuration.Bind<StorageOption>();
        Console.WriteLine($"SiloStartup: option={option}");
        option.Validate().Assert(x => x.IsOk(), option => $"StorageOption is invalid, errors={option.Error}");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<StorageOption>(option);
            services.AddSingleton<DatalakeOption>(option.Storage);
        });

        return builder;
    }

    public static void MapBlogApi(this IEndpointRouteBuilder app)
    {
        //app.ServiceProvider.GetRequiredService<StateManagementApi>().Setup(app);
    }
}

