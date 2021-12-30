using Directory.sdk.Service;
using DirectoryApi.Application;
using Microsoft.Extensions.Caching.Memory;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using Toolbox.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DirectoryApi
{
    public static class Startup
    {
        public static IServiceCollection ConfigureDirectoryService(this IServiceCollection service)
        {
            service.VerifyNotNull(nameof(service));

            service.AddSingleton<IDirectoryService, DirectoryService>();
            service.AddSingleton<IMemoryCache, MemoryCache>();

            service.AddSingleton<IDatalakeStore>(service =>
            {
                ApplicationOption option = service.GetRequiredService<ApplicationOption>();
                ILoggerFactory loggerFactory = service.GetRequiredService<ILoggerFactory>();

                var datalakeOption = new DatalakeStoreOption
                {
                    AccountName = option.Storage.AccountName,
                    ContainerName = option.Storage.ContainerName,
                    AccountKey = option.Storage.AccountKey,
                    BasePath = option.Storage.BasePath
                };

                return new DatalakeStore(datalakeOption, loggerFactory.CreateLogger<DatalakeStore>());
            });

            return service;
        }

        public static IApplicationBuilder ConfigureDirectoryService(this IApplicationBuilder app)
        {
            app.VerifyNotNull(nameof(app));

            ApplicationOption option = app.ApplicationServices.GetRequiredService<ApplicationOption>();
            app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, "/api/ping".ToEnumerable());

            app.ApplicationServices
                .GetRequiredService<IServiceStatus>()
                .SetStatus(ServiceStatusLevel.Ready, "Ready and running");

            return app;
        }
    }
}
