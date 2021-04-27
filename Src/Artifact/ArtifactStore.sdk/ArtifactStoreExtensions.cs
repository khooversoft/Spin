using ArtifactStore.sdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Actor.Host;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace ArtifactStore.sdk
{
    public static class ArtifactStoreExtensions
    {
        public static IServiceCollection AddArtifactStore(this IServiceCollection services)
        {
            services.VerifyNotNull(nameof(services));

            services.AddSingleton<IArtifactStoreFactory, ArtifactStoreFactory>();
            services.AddSingleton<IDataLakeStoreFactory, DataLakeStoreFactory>();

            return services;
        }
    }
}