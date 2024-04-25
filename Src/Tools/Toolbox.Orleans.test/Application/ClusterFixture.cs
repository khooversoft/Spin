using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
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
    public static IFileStore FileStore { get; } = new InMemoryFileStore(new NullLogger<InMemoryFileStore>());
    public ClusterFixture() => Cluster.Deploy();

    public TestCluster Cluster { get; } = new TestClusterBuilder()
        .AddSiloBuilderConfigurator<TestSiloConfigurations>()
        .Build();

    void IDisposable.Dispose() => Cluster.StopAllSilos();

    public IServiceProvider ServiceProvider => Cluster.ServiceProvider;
}

file sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(static services =>
        {
            services.AddLogging(config => config.AddDebug().AddConsole());
            services.AddSingleton<IFileStore>(ClusterFixture.FileStore);

            services.AddGrainFileStorage();
            services.AddStoreCollection((services, config) =>
            {
                config.Add(new StoreConfig("system", getFileStoreService));
                config.Add(new StoreConfig("contract", getFileStoreService));
            });
        });



        static IFileStore getFileStoreService(IServiceProvider services, StoreConfig config) => services.GetRequiredService<IFileStore>();
    }
}