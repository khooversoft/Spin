using Artifact.Application;
using ArtifactApi.Application;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace Artifact;

public static class Startup
{
    public static async Task<IServiceCollection> ConfigureArtifactService(this IServiceCollection service, ApplicationOption option)
    {
        service.NotNull(nameof(service));

        ApplicationOption applicationOption = await DirectoryTools.Run<ApplicationOption>(option.DirectoryUrl, option.DirectoryApiKey, async client =>
        {
            ServiceRecord serviceRecord = await client.GetServiceRecord(option.RunEnvironment, "Artifact");
            IReadOnlyList<StorageRecord> storageRecords = await client.GetStorageRecord(option.RunEnvironment, "Artifact");

            return option with
            {
                HostUrl = serviceRecord.HostUrl,
                ApiKey = serviceRecord.ApiKey,
                Storage = storageRecords,
            };
        });

        service.AddSingleton(applicationOption);

        service.AddSingleton<DocumentPackageFactory>(service =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            ILoggerFactory loggerFactory = service.GetRequiredService<ILoggerFactory>();

            var list = option.Storage
                .Select(x => (Container: x.Container, Option: new DatalakeStoreOption
                {
                    AccountName = x.AccountName,
                    ContainerName = x.ContainerName,
                    AccountKey = x.AccountKey,
                    BasePath = x.BasePath
                }));

            return new DocumentPackageFactory(list, loggerFactory);
        });

        return service;
    }

    public static IApplicationBuilder UseArtifactService(this IApplicationBuilder app)
    {
        app.NotNull(nameof(app));

        ApplicationOption option = app.ApplicationServices.GetRequiredService<ApplicationOption>();
        app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, new[] { "/api/ping" });

        app.ApplicationServices
            .GetRequiredService<IServiceStatus>()
            .SetStatus(ServiceStatusLevel.Ready, "Ready and running");

        return app;
    }
}
