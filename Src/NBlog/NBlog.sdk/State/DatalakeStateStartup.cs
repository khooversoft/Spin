using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Storage;
using Toolbox.Azure.DataLake;

namespace NBlog.sdk.State;

public static class DatalakeStateStartup
{
    public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDatalakeStore, DatalakeStore>();
            services.AddSingleton<DatalakeStateConnector>();
            services.AddSingletonNamedService("spinStateStore", CreateStorage);
        });

    private static IGrainStorage CreateStorage(IServiceProvider service, string name) => service.GetRequiredService<DatalakeStateConnector>();

}
