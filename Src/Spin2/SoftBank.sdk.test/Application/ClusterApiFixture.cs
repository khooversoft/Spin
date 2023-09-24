using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.Trx;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.test.Application;

public class ClusterApiFixture
{
    public ClusterApiFixture()
    {
        Option = new ConfigurationBuilder()
            .AddJsonFile("test-appsettings.json")
            .Build()
            .Bind<TestOption>().Assert(x => x.Validate().IsOk(), "Invalid");

        ServiceProvider = new ServiceCollection()
        .AddSingleton(Option)
        .Action(x =>
        {
            x.AddHttpClient<SubscriptionClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<TenantClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<UserClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SignatureClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<ContractClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SoftBankClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SoftBankTrxClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<AgentClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SmartcClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<ConfigClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
        })
        .BuildServiceProvider();
    }

    public TestOption Option { get; }

    public IServiceProvider ServiceProvider { get; }

    public HttpClient GetClient() => ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("raw");
}
