using Artifact.sdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace Artifact.sdk
{
    public static class ArtifactExtensions
    {
        public static IServiceCollection AddArtifactStore(this IServiceCollection services)
        {
            services.VerifyNotNull(nameof(services));

            //services.AddSingleton<IArtifactStoreFactory, ArtifactStoreFactory>();
            //services.AddSingleton<IDatalakeStoreFactory, DatalakeStoreFactory>();

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