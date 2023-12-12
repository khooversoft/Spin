using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Orleans.Runtime;

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
