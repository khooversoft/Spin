using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class AzureStartup
{
    public static IServiceCollection AddDatalakeFileStore(this IServiceCollection services, DatalakeOption datalakeOption, string? key = null)
    {
        datalakeOption.NotNull();
        datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption");

        switch (key.ToNullIfEmpty())
        {
            case null:
                services.AddSingleton(datalakeOption);
                services.AddSingleton<IFileStore, DatalakeStore>();
                break;

            case string vKey:
                services.AddKeyedSingleton(vKey, datalakeOption);
                services.AddKeyedSingleton<IFileStore, DatalakeStore>(vKey);
                break;
        }

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
