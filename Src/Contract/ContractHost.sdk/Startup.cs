using Contract.sdk.Client;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Tools;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Application;
using Toolbox.Tools;

namespace ContractHost.sdk;

public static class Startup
{
    public static async Task<IServiceCollection> ConfigureContractService(this IServiceCollection service, ContractHostOption option)
    {
        service.VerifyNotNull(nameof(service));

        ContractHostOption applicationOption = await DirectoryTools.Run(option.DirectoryUrl, option.DirectoryApiKey, async client =>
        {
            ServiceRecord contractRecord = await client.GetServiceRecord(option.RunEnvironment, "Contract");

            option = option with
            {
                ContractUrl = contractRecord.HostUrl,
                ContractApiKey = contractRecord.ApiKey,
            };

            return option.Verify();
        });

        service.AddSingleton(applicationOption);

        service.AddHttpClient<ContractClient>((service, httpClient) =>
        {
            ContractHostOption option = service.GetRequiredService<ContractHostOption>();
            httpClient.BaseAddress = new Uri(option.DirectoryUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.DirectoryApiKey);
        });

        return service;
    }
}