using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class AzureStartup
{
    public static IServiceCollection AddDatalakeFileStore(this IServiceCollection services, IConfigurationSection configuration)
    {
        configuration.NotNull();

        DatalakeOption datalakeOption = configuration.Get<DatalakeOption>().NotNull();
        datalakeOption.Validate().Assert(x => x.IsOk(), option => $"StorageOption is invalid, errors={option.Error}");

        services.AddSingleton(datalakeOption);
        services.AddSingleton<IDatalakeStore, DatalakeStore>();
        services.AddSingleton<IFileStore, DatalakeFileStoreConnector>();

        return services;
    }
}
