using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Identity.sdk.Actors;
using Identity.sdk.Client;
using Identity.sdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spin.Common.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Actor.Host;
using Toolbox.Tools;

namespace Identity.sdk
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection services, int actorCapacity = 10000)
        {
            services.VerifyNotNull(nameof(services));

            services.AddSingleton<ISignatureActor, SignatureActor>();
            //services.AddSingleton<ISubscriptionActor, SubscriptionActor>();
            services.AddSingleton<ITenantActor, TenantActor>();
            services.AddSingleton<IUserActor, UserActor>();

            services.AddSingleton<IArtifactClient, ArtifactClient>();
            services.AddSingleton<IIdentityStore, IdentityStore>();

            services.AddHttpClient<ArtifactClient>((service, http) =>
            {
                ArtifactStoreOption option = service.GetRequiredService<ArtifactStoreOption>();
                http.DefaultRequestHeaders.Add(option.GetApiHeader().Key, option.GetApiHeader().Value);
            });

            services.AddSingleton<IActorHost>(x =>
            {
                ILoggerFactory loggerFactory = x.GetRequiredService<ILoggerFactory>();

                IActorHost host = new ActorHost(actorCapacity, loggerFactory);
                host.Register<ISignatureActor>(() => x.GetRequiredService<ISignatureActor>());
                host.Register<ISubscriptionActor>(() => x.GetRequiredService<ISubscriptionActor>());
                host.Register<ITenantActor>(() => x.GetRequiredService<ITenantActor>());
                host.Register<IUserActor>(() => x.GetRequiredService<IUserActor>());

                return host;
            });

            return services;
        }
    }
}
