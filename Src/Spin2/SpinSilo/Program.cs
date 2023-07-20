using System.Reflection;
using Microsoft.Extensions.Hosting;
using SoftBank.sdk.Application;
using SpinCluster.sdk.Application;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
        silo.AddSpinCluster();
        silo.AddSoftBank();

    })
    .UseConsoleLifetime();

using IHost host = builder.Build();

await host.UseSpinCluster();

Console.WriteLine($"Spin Silo - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

await host.RunAsync();