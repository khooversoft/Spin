using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public static class SiloStartup
{
    public static ISiloBuilder AddTickShareCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        DatalakeOption datalakeOption = hostContext.Configuration.GetSection(TsConstants.DataLakeOptionConfigPath).Get<DatalakeOption>().NotNull();
        datalakeOption.Validate().Assert(x => x.IsOk(), option => $"StorageOption is invalid, errors={option.Error}");

        Console.WriteLine($"SiloStartup: option={datalakeOption}");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<DatalakeOption>(datalakeOption);
        });

        return builder;
    }

    public static void MapBlogApi(this IEndpointRouteBuilder app)
    {
        //app.ServiceProvider.GetRequiredService<StateManagementApi>().Setup(app);
    }
}

