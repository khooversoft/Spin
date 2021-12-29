using System.Collections.Generic;
using System.Linq;
using ArtifactStore.sdk.Services;
using Directory.sdk;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace ArtifactStore.sdk
{
    public static class ArtifactStoreExtensions
    {
        public static IServiceCollection AddArtifactStore(this IServiceCollection services)
        {
            services.VerifyNotNull(nameof(services));

            services.AddSingleton<IArtifactStoreFactory, ArtifactStoreFactory>();
            services.AddSingleton<IDatalakeStoreFactory, DatalakeStoreFactory>();

            //services.AddSingleton<IReadOnlyList<DatalakeContainerOption>>(service =>
            //{
            //    IReadOnlyList<DatalakeContainerOption> data = service
            //        .GetRequiredService<IDirectoryNameService>()
            //        .Default
            //        .Storage.Values
            //        .Select(x =>
            //            new DatalakeContainerOption()
            //            {
            //                Name = x.StorageId,
            //                PathRoot = x.PathRoot,
            //                Store = new DatalakeStoreOption()
            //                {
            //                    ContainerName = x.ContainerName,
            //                    AccountName = x.AccountName,
            //                    AccountKey = x.AccountKey
            //                }
            //            }
            //        )
            //        .ToArray();

            //    return data;
            //});

            return services;
        }
    }
}