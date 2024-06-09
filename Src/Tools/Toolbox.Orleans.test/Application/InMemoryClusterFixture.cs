using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using Toolbox.Graph;
using Toolbox.Store;

namespace Toolbox.Orleans.test.Application;

public sealed class InMemoryClusterFixture : IDisposable
{
    public static IGraphFileStore FileStore { get; } = new InMemoryGraphFileStore(new NullLogger<InMemoryFileStore>());
    public InMemoryClusterFixture() => Cluster.Deploy();

    public TestCluster Cluster { get; } = new TestClusterBuilder()
        .AddSiloBuilderConfigurator<TestSiloConfigurations>()
        .Build();

    void IDisposable.Dispose() => Cluster.StopAllSilos();
}

file sealed class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(static services =>
        {
            services.AddLogging(config => config.AddDebug().AddConsole());
            services.AddSingleton<IFileStore>(InMemoryClusterFixture.FileStore);

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