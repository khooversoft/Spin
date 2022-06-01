using Artifact.sdk;
using ContractApi.Application;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Directory.sdk.Tools;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace ContractApi;

public static class Startup
{
    public static async Task<IServiceCollection> ConfigureContractService(this IServiceCollection service, ApplicationOption option)
    {
        service.NotNull();

        ApplicationOption applicationOption = await DirectoryTools.Run(option.DirectoryUrl, option.DirectoryApiKey, async client =>
        {
            ServiceRecord contractRecord = await client.GetServiceRecord(option.RunEnvironment, "Contract");
            ServiceRecord artifactRecord = await client.GetServiceRecord(option.RunEnvironment, "Artifact");

            option = option with
            {
                HostUrl = contractRecord.HostUrl,
                ApiKey = contractRecord.ApiKey,

                ArtifactUrl = artifactRecord.HostUrl,
                ArtifactApiKey = artifactRecord.ApiKey,
            };

            return option.Verify();
        });

        service.AddSingleton(applicationOption);
        service.AddSingleton<ContractService>();

        service.AddHttpClient<SigningClient>((service, httpClient) =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            httpClient.BaseAddress = new Uri(option.DirectoryUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.DirectoryApiKey);
        });

        service.AddHttpClient<ArtifactClient>((service, httpClient) =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            httpClient.BaseAddress = new Uri(option.ArtifactUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ArtifactApiKey);
        });

        return service;
    }

    public static IApplicationBuilder UseContractService(this IApplicationBuilder app)
    {
        app.NotNull();

        ApplicationOption option = app.ApplicationServices.GetRequiredService<ApplicationOption>();
        app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, new[] { "/api/ping" });

        app.ApplicationServices
            .GetRequiredService<IServiceStatus>()
            .SetStatus(ServiceStatusLevel.Ready, "Ready and running");

        return app;
    }
}
