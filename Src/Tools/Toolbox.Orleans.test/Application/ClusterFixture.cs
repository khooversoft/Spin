using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Orleans.TestingHost;
using Toolbox.Azure;
using Toolbox.Store;

namespace Toolbox.Orleans.test.Application;

public sealed class ClusterFixture : IDisposable
{
    public ClusterFixture() => Cluster.Deploy();

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
            services.AddLogging();
            services.AddSingleton<IFileStore, InMemoryFileStore>();
            services.AddSingleton<GrainStorageFileStoreConnector>();

            services.AddStoreCollection((services, config) =>
            {
                config.Add(new StoreConfig("directory", (services, config) =>
                {
                    return services.GetRequiredService<IFileStore>();
                }));
            });

            services.AddKeyedSingleton<IGrainStorage, GrainStorageFileStoreConnector>(OrleansConstants.StorageProviderName);
        });
    }
}