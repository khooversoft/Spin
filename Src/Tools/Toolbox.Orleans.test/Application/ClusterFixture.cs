using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.Storage;
using Orleans.TestingHost;
using Toolbox.Azure;
using Toolbox.Store;

namespace Toolbox.Orleans.test.Application;

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
            services.AddSingleton<GrainStorageFileStoreConnector>();
            services.AddSingleton<IStoreCollection, StoreCollection>();

            services.AddStoreCollection((services, config) =>
            {
                config.Add(new StoreConfig(OrleansConstants.DirectoryActorKey, (services, config) =>
                {
                    return services.GetRequiredService<IFileStore>();
                }));
            });

            services.AddKeyedSingleton<IGrainStorage, GrainStorageFileStoreConnector>(OrleansConstants.StorageProviderName);
        });
    }
}