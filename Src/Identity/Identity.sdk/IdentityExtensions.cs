using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Store;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Identity.sdk
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            services.VerifyNotNull(nameof(services));

            services.AddSingleton(s => new SignatureStore(s.GetRequiredService<IArtifactClient>(), s.GetRequiredService<IdentityNamespaces>().Signature));
            services.AddSingleton(s => new SubscriptionStore(s.GetRequiredService<IArtifactClient>(), s.GetRequiredService<IdentityNamespaces>().Subscription));
            services.AddSingleton(s => new TenantStore(s.GetRequiredService<IArtifactClient>(), s.GetRequiredService<IdentityNamespaces>().Tenant));
            services.AddSingleton(s => new UserStore(s.GetRequiredService<IArtifactClient>(), s.GetRequiredService<IdentityNamespaces>().User));

            services.AddHttpClient<IArtifactClient, ArtifactClient>((service, http) =>
            {
                ArtifactStoreOption option = service.GetRequiredService<ArtifactStoreOption>();
                http.DefaultRequestHeaders.Add(option.GetApiHeader().Key, option.GetApiHeader().Value);
            });

            return services;
        }
    }
}