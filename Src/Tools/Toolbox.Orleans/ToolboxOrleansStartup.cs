using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace Toolbox.Orleans;

public static class ToolboxOrleansStartup
{
    public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder, string providerName = "datalake") =>
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDatalakeStore, DatalakeStore>();
            services.AddSingleton<DatalakeStateConnector>();
            services.AddKeyedSingleton<IGrainStorage>(providerName.NotEmpty(), CreateStorage);
        });

    private static IGrainStorage CreateStorage(IServiceProvider service, object _) => service.GetRequiredService<DatalakeStateConnector>();

}
