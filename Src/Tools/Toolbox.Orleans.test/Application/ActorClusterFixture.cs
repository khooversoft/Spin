using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.TestingHost;
using Toolbox.Azure;
using Toolbox.Configuration;
using Toolbox.Orleans.Testing;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Orleans.test.Application;

public sealed class ActorClusterFixture : IDisposable
{
    public static DatalakeOption DatalakeOption;

    static ActorClusterFixture()
    {
        var config = new ConfigurationBuilder()
            .AddResourceStream(typeof(ActorClusterFixture), "Toolbox.Orleans.test.Application.test-appsettings.json")
            .AddUserSecrets("Toolbox-Azure-test")
            .Build();

        string datalakeConnectionString = config["datalakeConnectionString"].NotEmpty();
        string clientSecretConnectionString = config["clientSecretConnectionString"].NotEmpty();

        DatalakeOption = DatalakeOptionTool.Create(datalakeConnectionString, clientSecretConnectionString);
    }

    public ActorClusterFixture()
    {
        Cluster.Deploy();
        var clean = Cluster.ServiceProvider.GetRequiredService<CleanDirectoryAction>();
        var result = clean.Clean().Result;
    }

    public TestCluster Cluster { get; } = new TestClusterBuilder()
        .AddSiloBuilderConfigurator<TestSiloConfigurations>()
        .AddClientBuilderConfigurator<TestClientConfiguration>()
        .Build();

    void IDisposable.Dispose() => Cluster.StopAllSilos();
}

file sealed class TestClientConfiguration : IClientBuilderConfigurator
{
    public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
    {
        clientBuilder.Services.AddSingleton<UserStore>();
        clientBuilder.Services.AddSingleton<CleanDirectoryAction>();
    }
}

file sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {

        siloBuilder.ConfigureServices(services =>
        {
            services.AddLogging(config => config.AddDebug().AddConsole());
            services.AddSingleton(ActorClusterFixture.DatalakeOption);
            services.AddSingleton<IDatalakeStore, DatalakeStore>();
            services.AddSingleton<IFileStore, DatalakeFileStoreConnector>();

            services.AddGrainFileStore();
            services.AddStoreCollection((services, config) =>
            {
                config.Add("system", getFileStoreService);
                config.Add("contract", getFileStoreService);
                config.Add("nodes", getFileStoreService);
            });
        });

        static IFileStore getFileStoreService(IServiceProvider services, StoreConfig _) => services.GetRequiredService<IFileStore>();
    }
}
