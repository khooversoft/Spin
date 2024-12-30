using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Types;

namespace TicketShare.sdk;

public static class TicketShareStartup
{
    //public static void AddAzureApplicationConfiguration(this IHostApplicationBuilder builder)
    //{
    //    string connectionString = builder.Configuration.GetConnectionString("AppConfig").NotNull();
    //    ClientSecretCredential credential = ClientCredential.ToClientSecretCredential(connectionString);

    //    var appConfigEndpoint = "https://biz-bricks-prod-configuration.azconfig.io";

    //    // Build configuration
    //    builder.Configuration.AddAzureAppConfiguration(options =>
    //    {
    //        options.Connect(new Uri(appConfigEndpoint), credential)
    //            .ConfigureKeyVault(kv =>
    //            {
    //                kv.SetCredential(credential);
    //            })
    //            .Select(TsConstants.ConfigurationFilter, LabelFilter.Null)
    //            .Select(TsConstants.ConfigurationFilter, builder.Environment.EnvironmentName);
    //    });

    //    builder.Configuration.AddPropertyResolver();
    //}

    public static IServiceCollection AddTicketShare(this IServiceCollection service)
    {
        service.AddToolboxIdentity();
        service.AddSingleton<AccountClient>();
        service.AddScoped<UserAccountManager>();

        service.AddSingleton<TicketGroupClient>();
        service.AddSingleton<TicketGroupProposalClient>();
        service.AddSingleton<TicketGroupSearchClient>();
        service.AddScoped<TicketGroupManager>();

        service.AddSingleton<HubChannelClient>();
        service.AddSingleton<HubChannelMessageClient>();
        return service;
    }
}

public class TicketShareTestHost
{
    private GraphTestClient _testClient;

    public TicketShareTestHost()
    {
        _testClient = GraphTestStartup.CreateGraphTestHost(null, service =>
        {
            service.AddTicketShare();
        });
    }

    public IGraphClient TestClient => _testClient;

    public IServiceProvider ServiceProvider => _testClient.ServiceProvider;
    public ScopeContext GetScopeContext<T>() => _testClient.GetScopeContext<T>();
}
