using Microsoft.Extensions.DependencyInjection;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class AzureStartup
{
    public static IServiceCollection AddDatalakeFileStore(this IServiceCollection services, DatalakeOption datalakeOption)
    {
        datalakeOption.NotNull();
        datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption");

        services.AddSingleton(datalakeOption);
        services.AddSingleton<IDatalakeStore, DatalakeStore>();
        services.AddSingleton<IFileStore, DatalakeFileStoreConnector>();

        return services;
    }
}
