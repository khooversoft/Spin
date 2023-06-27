using ObjectStore.sdk.Client;
using Polly;
using Polly.Extensions.Http;
using SpinCluster.sdk.Client;
using Toolbox.Extensions;

namespace SpinPortal.Application;

public static class Startup
{
    public static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .RetryAsync(5);

    public static void AddPortal(this WebApplicationBuilder builder)
    {
        PortalOption option = builder
            .Configuration
            .Bind<PortalOption>()
            .Verify();

        builder.Services.AddSingleton(option);
        builder.Services.AddScoped<JsRunTimeService>();

        builder.Services.AddHttpClient<SpinClusterClient>((services, httpClient) =>
        {
            var option = services.GetRequiredService<PortalOption>();
            httpClient.BaseAddress = new Uri(option.SpinSiloApi);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        });
        //.AddPolicyHandler(_retryPolicy);

        builder.Services.AddHttpClient<SpinLeaseClient>((services, httpClient) =>
        {
            var option = services.GetRequiredService<PortalOption>();
            httpClient.BaseAddress = new Uri(option.SpinSiloApi);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        });
        //.AddPolicyHandler(_retryPolicy);

        builder.Services.AddHttpClient<SpinConfigurationClient>((services, httpClient) =>
        {
            var option = services.GetRequiredService<PortalOption>();
            httpClient.BaseAddress = new Uri(option.SpinSiloApi);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        });
        //.AddPolicyHandler(_retryPolicy);
    }
}
