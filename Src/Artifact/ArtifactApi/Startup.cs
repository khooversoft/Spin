using Artifact.Application;
using ArtifactApi.Application;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
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
using Toolbox.Document;
using Toolbox.Tools;

namespace Artifact;

public static class Startup
{
    public static async Task<IServiceCollection> ConfigureArtifactService(this IServiceCollection service, ApplicationOption option)
    {
        service.VerifyNotNull(nameof(service));

        ApplicationOption applicationOption = await DirectoryClient.Run<ApplicationOption>(option.DirectoryUrl, option.DirectoryApiKey, async client =>
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

    public static IApplicationBuilder ConfigureArtifactService(this IApplicationBuilder app)
    {
        app.VerifyNotNull(nameof(app));

        ApplicationOption option = app.ApplicationServices.GetRequiredService<ApplicationOption>();
        app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, new[] { "/api/ping" });

        app.ApplicationServices
            .GetRequiredService<IServiceStatus>()
            .SetStatus(ServiceStatusLevel.Ready, "Ready and running");

        return app;
    }
}
