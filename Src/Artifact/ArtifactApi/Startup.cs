//using Artifact.Application;
//using Directory.sdk.Client;
//using Directory.sdk.Model;
//using Directory.sdk.Service;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Spin.Common.Middleware;
//using Spin.Common.Model;
//using Spin.Common.Services;
//using System.Threading.Tasks;
//using Toolbox.Application;
//using Toolbox.Azure.DataLake;
//using Toolbox.Azure.DataLake.Model;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Artifact;

//public static class Startup
//{
//    public static async Task<IServiceCollection> ConfigureArtifactService(this IServiceCollection service, ApplicationOption option)
//    {
//        service.VerifyNotNull(nameof(service));

//        ApplicationOption applicationOption = await DirectoryHost.Run<ApplicationOption>(option.DirectoryUrl, option.DirectoryApiKey, async client =>
//        {
//            ServiceRecord serviceRecord = await client.GetServiceRecord(option.RunEnvironment, "Artifact");
//            StorageRecord storageRecord = await client.GetStorageRecord(option.RunEnvironment, "Artifact");

//            return option with
//            {
//                HostUrl = serviceRecord.HostUrl,
//                ApiKey = serviceRecord.ApiKey,
//                Storage = new StorageOption
//                {
//                    AccountName = storageRecord.AccountName,
//                    ContainerName = storageRecord.ContainerName,
//                    AccountKey = storageRecord.AccountKey,
//                    BasePath = storageRecord.BasePath,
//                }
//            };
//        });

//        service.AddSingleton(applicationOption);
//        service.AddSingleton<IDirectoryService, DirectoryService>();
//        service.AddSingleton<IMemoryCache, MemoryCache>();
//        service.AddSingleton<IArtifactStore, Artifact.sdk.Services.ArtifactStore>();

//        service.AddSingleton<IDatalakeStore>(service =>
//        {
//            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
//            ILoggerFactory loggerFactory = service.GetRequiredService<ILoggerFactory>();

//            var datalakeOption = new DatalakeStoreOption
//            {
//                AccountName = option.Storage.AccountName,
//                ContainerName = option.Storage.ContainerName,
//                AccountKey = option.Storage.AccountKey,
//                BasePath = option.Storage.BasePath
//            };

//            return new DatalakeStore(datalakeOption, loggerFactory.CreateLogger<DatalakeStore>());
//        });

//        return service;
//    }

//    public static IApplicationBuilder ConfigureArtifactService(this IApplicationBuilder app)
//    {
//        app.VerifyNotNull(nameof(app));

//        ApplicationOption option = app.ApplicationServices.GetRequiredService<ApplicationOption>();
//        app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, "/api/ping".ToEnumerable());

//        app.ApplicationServices
//            .GetRequiredService<IServiceStatus>()
//            .SetStatus(ServiceStatusLevel.Ready, "Ready and running");

//        return app;
//    }
//}
