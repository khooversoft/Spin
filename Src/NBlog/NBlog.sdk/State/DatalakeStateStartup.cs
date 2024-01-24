using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Storage;
using Toolbox.Azure.DataLake;

namespace NBlog.sdk;

public static class DatalakeStateStartup
{
    public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDatalakeStore, DatalakeStore>();
            services.AddSingleton<DatalakeStateConnector>();
            services.AddKeyedSingleton<IGrainStorage>(NBlogConstants.DataLakeProviderName, CreateStorage);
        });

    private static IGrainStorage CreateStorage(IServiceProvider service, object _) => service.GetRequiredService<DatalakeStateConnector>();

}
