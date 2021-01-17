using ArtifactStore.sdk.Actors;
using ArtifactStore.sdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Actor.Host;
using Toolbox.Tools;

namespace ArtifactStore.sdk
{
    public static class ArtifactStoreExtensions
    {
        public static IServiceCollection AddArtifactStore(this IServiceCollection services, int actorCapacity = 10000)
        {
            services.VerifyNotNull(nameof(services));

            services.AddSingleton<IArtifactStorageFactory, ArtifactStorageFactory>();
            services.AddSingleton<IArtifactStoreService, ArtifactStoreService>();

            services.AddTransient<IArtifactPayloadActor, ArtifactPayloadActor>();

            services.AddSingleton<IActorHost>(x =>
            {
                ILoggerFactory loggerFactory = x.GetRequiredService<ILoggerFactory>();

                IActorHost host = new ActorHost(actorCapacity, loggerFactory);
                host.Register<IArtifactPayloadActor>(() => x.GetRequiredService<IArtifactPayloadActor>());

                return host;
            });

            return services;
        }
    }
}