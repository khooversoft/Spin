using Contract.sdk.Client;
using Microsoft.Extensions.DependencyInjection;
using Smart_Installment.sdk.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Actor.Host;

namespace Smart_Installment.sdk.Test.Application;

internal class TestHost
{
    private IServiceProvider? _serviceProvider;

    public static TestHost Instance { get; } = new TestHost();



    public async Task<IServiceProvider> GetServices()
    {
        return _serviceProvider ??= (await start());


        async Task<IServiceProvider> start()
        {
            ApplicationOption option = (await TestingConfiguration.Instance.GetConfiguration()).NotNull();

            var builder = new ServiceCollection();
            builder.AddSingleton(option);
            builder.AddTransient<IContractStoreActor, ContractStoreActor>();

            builder.AddActor(config =>
            {
                config.Register<IContractStoreActor>();
            });

            builder.AddHttpClient<ContractClient>((service, httpClient) =>
            {
                ApplicationOption option = service.GetRequiredService<ApplicationOption>();
                httpClient.BaseAddress = new Uri(option.ContractUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ContractApiKey);
            });

            return builder.BuildServiceProvider();
        };
    }
}
