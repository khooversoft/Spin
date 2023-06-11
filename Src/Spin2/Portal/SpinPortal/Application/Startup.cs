using ObjectStore.sdk.Client;
using Polly;
using Polly.Extensions.Http;
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
        builder.Services.AddSingleton<ClipboardService>();

        builder.Services.AddHttpClient<ObjectStoreClient>((services, httpClient) =>
        {
            var option = services.GetRequiredService<PortalOption>();

            httpClient.BaseAddress = new Uri(option.DirectoryUri);
        });
        //.AddPolicyHandler(_retryPolicy);
    }
}
