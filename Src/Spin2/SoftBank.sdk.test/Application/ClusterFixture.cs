using Orleans.TestingHost;

namespace SoftBank.sdk.test.Application;

public class ClusterFixture : IDisposable
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose() => Cluster.StopAllSilos();

    public TestCluster Cluster { get; }

    private class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            throw new NotImplementedException();
            //siloBuilder.AddSpinCluster("test-appsettings.json");
            //siloBuilder.AddSoftBank();
        }
    }
}