//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Abstractions;
//using Orleans.TestingHost;
//using Toolbox.Store;

//namespace Toolbox.Orleans.test.Application;

//public sealed class InMemoryClusterFixture : IDisposable
//{
//    public static IFileStore FileStore { get; } = new InMemoryFileStore(new NullLogger<InMemoryFileStore>());
//    public InMemoryClusterFixture() => Cluster.Deploy();

//    public TestCluster Cluster { get; } = new TestClusterBuilder()
//        .AddSiloBuilderConfigurator<TestSiloConfigurations>()
//        .Build();

//    void IDisposable.Dispose() => Cluster.StopAllSilos();
//}

//file sealed class TestSiloConfigurations : ISiloConfigurator
//{
//    public void Configure(ISiloBuilder siloBuilder)
//    {
//        siloBuilder.ConfigureServices(static services =>
//        {
//            services.AddLogging(config => config.AddDebug().AddConsole());
//            services.AddSingleton<IFileStore>(InMemoryClusterFixture.FileStore);

//            services.AddGrainFileStore();
//            services.AddStoreCollection((services, config) =>
//            {
//                config.Add("system", getFileStoreService);
//                config.Add("contract", getFileStoreService);
//                config.Add("nodes", getFileStoreService);
//            });
//        });

//        static IFileStore getFileStoreService(IServiceProvider services, StoreConfig config) => services.GetRequiredService<IFileStore>();
//    }
//}