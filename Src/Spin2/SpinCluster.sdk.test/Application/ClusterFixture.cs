using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Orleans.TestingHost;
using SpinCluster.sdk.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Types;
using SpinCluster.sdk.State;
using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Services;
using Microsoft.Extensions.Hosting;
using FluentAssertions.Common;
using Toolbox.Tools;

namespace SpinCluster.sdk.test.Application;

public class ClusterFixture : IDisposable
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        Cluster = builder.Build();
        Cluster.Deploy();

        //((InProcessSiloHandle)Cluster.Primary).SiloHost.Services.UseSpinCluster().GetAwaiter().GetResult();
    }

    public void Dispose() => Cluster.StopAllSilos();

    public TestCluster Cluster { get; }

    private class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddSpinCluster("test-appsettings.json");

            //siloBuilder.AddStartupTask(async (IServiceProvider services, CancellationToken _) => await services.UseSpinCluster());

            //siloBuilder.AddDatalakeGrainStorage(option);
            //siloBuilder.ConfigureServices(services =>
            //{
            //    services.AddSpinCluster(option);
            //});
        }

        //private static SpinClusterOption ReadOption() => new ConfigurationBuilder()
        //    .AddJsonFile("test-appsettings.json")
        //    .AddUserSecrets("Toolbox-Azure-test")
        //    .Build()
        //    .Bind<SpinClusterOption>()
        //    .Verify();
    }
}