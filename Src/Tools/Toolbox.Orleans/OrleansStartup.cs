using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public static class OrleansStartup
{
    public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IGrainStorage, DatalakeGrainStorageConnector>(OrleansConstants.StorageProviderName);
        });
}
