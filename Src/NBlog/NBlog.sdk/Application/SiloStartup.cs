using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class SiloStartup
{
    public static ISiloBuilder AddBlogCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        StorageOption option = hostContext.Configuration.Bind<StorageOption>();
        option.Validate(out Option v).Assert(x => x == true, $"StorageOption is invalid, errors={v.Error}");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<DatalakeOption>(option.Storage);
            services.AddSingleton<ArticleService>();
            services.AddSingleton<ManifestService>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<ConfigurationService>();
            services.AddSingleton<StorageService>();

            services.AddSingleton<ArticleDirectoryClient>(service =>
            {
                IClusterClient clusterClient = service.GetRequiredService<IClusterClient>();
                IDirectoryActor directoryActor = clusterClient.GetDirectoryActor();

                return ActivatorUtilities.CreateInstance<ArticleDirectoryClient>(service, directoryActor);
            });
        });

        return builder;
    }
}

