using Directory.sdk.Service;
using DirectoryApi.Application;
using Microsoft.Extensions.Caching.Memory;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using Toolbox.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DirectoryApi;

public static class Startup
{
    public static IServiceCollection ConfigureDirectoryService(this IServiceCollection service)
    {
        service.NotNull(nameof(service));

        service.AddSingleton<IMemoryCache, MemoryCache>();

        service.AddSingleton<DirectoryService>(service =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            ILoggerFactory loggerFactory = service.GetRequiredService<ILoggerFactory>();
            IMemoryCache memoryCache = service.GetRequiredService<IMemoryCache>();

            var datalakeOption = new DatalakeStoreOption
            {
                AccountName = option.Storage.AccountName,
                ContainerName = option.Storage.ContainerName,
                AccountKey = option.Storage.AccountKey,
                BasePath = option.Storage.BasePath
            };

            var store = new DatalakeStore(datalakeOption, loggerFactory.CreateLogger<DatalakeStore>());
            var document = new DocumentStorage(store, memoryCache);
            return new DirectoryService(document);
        });

        service.AddSingleton<IdentityService>(service =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            ILoggerFactory loggerFactory = service.GetRequiredService<ILoggerFactory>();
            IMemoryCache memoryCache = service.GetRequiredService<IMemoryCache>();

            var datalakeOption = new DatalakeStoreOption
            {
                AccountName = option.IdentityStorage.AccountName,
                ContainerName = option.IdentityStorage.ContainerName,
                AccountKey = option.IdentityStorage.AccountKey,
                BasePath = option.IdentityStorage.BasePath
            };

            var store = new DatalakeStore(datalakeOption, loggerFactory.CreateLogger<DatalakeStore>());
            var document = new DocumentStorage(store, memoryCache);
            return new IdentityService(document, loggerFactory.CreateLogger<IdentityService>());
        });

        service.AddSingleton<SigningService>(service =>
        {
            DirectoryService directoryService = service.GetRequiredService<DirectoryService>();
            IdentityService identityService = service.GetRequiredService<IdentityService>();
            ILoggerFactory loggerFactory = service.GetRequiredService<ILoggerFactory>();

            return new SigningService(directoryService, identityService, loggerFactory.CreateLogger<SigningService>());
        });

        return service;
    }

    public static IApplicationBuilder UseDirectoryService(this IApplicationBuilder app)
    {
        app.NotNull(nameof(app));

        ApplicationOption option = app.ApplicationServices.GetRequiredService<ApplicationOption>();
        app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, "/api/ping".ToEnumerable());

        app.ApplicationServices
            .GetRequiredService<IServiceStatus>()
            .SetStatus(ServiceStatusLevel.Ready, "Ready and running");

        return app;
    }
}
