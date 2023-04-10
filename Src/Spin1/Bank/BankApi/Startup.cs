using Artifact.sdk;
using Bank.sdk;
using Bank.sdk.Service;
using BankApi.Application;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Tools;
using Spin.Common.Middleware;
using Spin.Common.Model;
using Spin.Common.Services;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace BankApi;

public static class Startup
{
    public static async Task<IServiceCollection> ConfigureBankService(this IServiceCollection service, ApplicationOption option)
    {
        service.NotNull();

        ApplicationOption applicationOption = await DirectoryTools.Run<ApplicationOption>(option.DirectoryUrl, option.DirectoryApiKey, async client =>
        {
            BankServiceRecord serviceRecord = await client.GetBankServiceRecord(option.RunEnvironment, option.BankName);
            ServiceRecord artifactRecord = await client.GetServiceRecord(option.RunEnvironment, "Artifact");

            option = option with
            {
                BankContainer = serviceRecord.Container,
                HostUrl = serviceRecord.HostUrl,
                ApiKey = serviceRecord.ApiKey,
                ArtifactUrl = artifactRecord.HostUrl,
                ArtifactApiKey = artifactRecord.ApiKey,
            };

            return option.Verify();
        });

        service.AddSingleton(applicationOption);

        service.AddBankHost(new BankOption
        {
            RunEnvironment = option.RunEnvironment,
            BankName = option.BankName,
            ArtifactContainerName = applicationOption.BankContainer
        });

        service.AddHttpClient<ArtifactClient>((service, httpClient) =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            httpClient.BaseAddress = new Uri(option.ArtifactUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ArtifactApiKey);
        });

        service.AddHttpClient<DirectoryClient>((service, httpClient) =>
        {
            ApplicationOption option = service.GetRequiredService<ApplicationOption>();
            httpClient.BaseAddress = new Uri(option.DirectoryUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.DirectoryApiKey);
        });


        return service;
    }

    public static IApplicationBuilder UseBankService(this IApplicationBuilder app)
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
