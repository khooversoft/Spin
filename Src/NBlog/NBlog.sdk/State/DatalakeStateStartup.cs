using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Storage;

namespace NBlog.sdk.State;

public static class DatalakeStateStartup
{
    public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<DatalakeStateConnector>();
            services.AddSingletonNamedService("spinStateStore", CreateStorage);
        });

    private static IGrainStorage CreateStorage(IServiceProvider service, string name) => service.GetRequiredService<DatalakeStateConnector>();

}
