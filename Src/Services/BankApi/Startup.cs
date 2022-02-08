using Artifact.sdk;
using Bank.sdk.Service;
using BankApi.Application;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Tools;

namespace BankApi;

public static class Startup
{
    public static async Task<IServiceCollection> ConfigureBankService(this IServiceCollection service, ApplicationOption option)
    {
        service.VerifyNotNull(nameof(service));

        ApplicationOption applicationOption = await DirectoryClient.Run<ApplicationOption>(option.DirectoryUrl, option.DirectoryApiKey, async client =>
        {
            ServiceRecord serviceRecord = await client.GetServiceRecord(option.RunEnvironment, "Bank");
            ServiceRecord artifactRecord = await client.GetServiceRecord(option.RunEnvironment, "Artifact");

            option = option with
            {
                HostUrl = serviceRecord.HostUrl,
                ApiKey = serviceRecord.ApiKey,
                ArtifactUrl = artifactRecord.HostUrl,
                ArtifactApiKey = artifactRecord.ApiKey,
            };

            return option.Verify();
        });

        service.AddSingleton(applicationOption);
        service.AddSingleton<BankAccountService>();
        service.AddSingleton<BankTransactionService>();

        service.AddHttpClient<ArtifactClient>((service, httpClient) =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            httpClient.BaseAddress = new Uri(option.ArtifactUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ArtifactApiKey);
        });

        return service;
    }

    public static IApplicationBuilder ConfigureBankService(this IApplicationBuilder app)
    {
        app.VerifyNotNull(nameof(app));

        ApplicationOption option = app.ApplicationServices.GetRequiredService<ApplicationOption>();
        app.UseMiddleware<ApiKeyMiddleware>(Constants.ApiKeyName, option.ApiKey, new[] { "/api/ping" });

        app.ApplicationServices
            .GetRequiredService<IServiceStatus>()
            .SetStatus(ServiceStatusLevel.Ready, "Ready and running");

        return app;
    }
}
