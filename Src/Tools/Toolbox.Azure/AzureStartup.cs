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
        services.AddSingleton<IKeyStore, DatalakeStore>();

        return services;
    }

    public static IServiceCollection AddDatalakeKeyedFileStore(this IServiceCollection services, string key, DatalakeOption datalakeOption)
    {
        key.NotEmpty();
        datalakeOption.NotNull();
        datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption");

        services.AddKeyedSingleton(key, datalakeOption);
        services.AddKeyedSingleton<IKeyStore, DatalakeStore>(key, (services, _) =>
        {
            var option = services.GetRequiredKeyedService<DatalakeOption>(key);
            return ActivatorUtilities.CreateInstance<DatalakeStore>(services, option);
        });

        return services;
    }

    //public static GraphHostBuilder AddDatalakeFileStore(this GraphHostBuilder graphHostService, DatalakeOption datalakeOption)
    //{
    //    graphHostService.NotNull();
    //    datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption");

    //    graphHostService.AddServiceConfiguration(x => x.AddDatalakeFileStore(datalakeOption));
    //    return graphHostService;
    //}

    //public static GraphHostBuilder AddDatalakeFileStore(this GraphHostBuilder graphHostService)
    //{
    //    graphHostService.NotNull();

    //    graphHostService.AddServiceConfiguration(x =>
    //    {
    //        x.AddSingleton<DatalakeOption>(service =>
    //        {
    //            IConfiguration config = service.GetRequiredService<IConfiguration>();
    //            DatalakeOption datalakeOption = config.GetSection("Storage").Get<DatalakeOption>().NotNull();
    //            datalakeOption.Validate().BeOk();

    //            return datalakeOption;
    //        });

    //        x.AddSingleton<IFileStore, DatalakeStore>();
    //    });

    //    return graphHostService;
    //}
}
