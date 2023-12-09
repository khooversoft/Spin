using LoanContract.sdk.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.Trx;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.test.Application;

public class ClusterApiFixture
{
    public ClusterApiFixture()
    {
        Option = new ConfigurationBuilder()
            .AddJsonFile("test-appsettings.json")
            .Build()
            .Bind<ClientOption>().Assert(x => x.Validate().IsOk(), "Invalid");

        ServiceProvider = new ServiceCollection()
        .AddSingleton(Option)
        .AddSingleton<LoanContractManager>()
        .Action(x =>
        {
            x.AddHttpClient<ConfigClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<ContractClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<LeaseClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<PrincipalKeyClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<PrincipalPrivateKeyClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SchedulerClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<ScheduleWorkClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SignatureClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SmartcClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SoftBankClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SoftBankTrxClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<SubscriptionClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<TenantClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<UserClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
            x.AddHttpClient<AgentClient>(client => client.BaseAddress = new Uri(Option.ClusterApiUri));
        })
        .BuildServiceProvider();
    }

    public ClientOption Option { get; }

    public IServiceProvider ServiceProvider { get; }

    public HttpClient GetClient() => ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("raw");
}