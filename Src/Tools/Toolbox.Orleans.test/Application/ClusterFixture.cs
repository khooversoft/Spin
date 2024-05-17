using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using Toolbox.Graph;
using Toolbox.Store;

namespace Toolbox.Orleans.test.Application;

//[CollectionDefinition("ClusterFixture")]
//public class ClusterCollection : ICollectionFixture<ClusterFixture>
//{
//    // This class has no code, and is never created. Its purpose is simply
//    // to be the place to apply [CollectionDefinition] and all the
//    // ICollectionFixture<> interfaces.
//}

public sealed class ClusterFixture : IDisposable
{
    public static IGraphFileStore FileStore { get; } = new InMemoryGraphFileStore(new NullLogger<InMemoryFileStore>());
    public ClusterFixture() => Cluster.Deploy();

    public TestCluster Cluster { get; } = new TestClusterBuilder()
        .AddSiloBuilderConfigurator<TestSiloConfigurations>()
        .AddClientBuilderConfigurator<TestClientConfiguration>()
        .Build();

    void IDisposable.Dispose() => Cluster.StopAllSilos();

    public IServiceProvider ServiceProvider => Cluster.ServiceProvider;
}

file sealed class TestClientConfiguration : IClientBuilderConfigurator
{
    public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
    {
        //clientBuilder.Services.AddDirectoryClient();
    }
}

sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(static services =>
        {
            services.AddLogging(config => config.AddDebug().AddConsole());
            services.AddSingleton<IFileStore>(ClusterFixture.FileStore);
            services.AddSingleton<IGraphFileStore>(ClusterFixture.FileStore);

            services.AddGrainFileStore();
            services.AddStoreCollection((services, config) =>
            {
                config.Add(new StoreConfig("system", getFileStoreService));
                config.Add(new StoreConfig("contract", getFileStoreService));
                config.Add(new StoreConfig("nodes", getFileStoreService));
            });
        });

        static IFileStore getFileStoreService(IServiceProvider services, StoreConfig config) => services.GetRequiredService<IFileStore>();
    }
}