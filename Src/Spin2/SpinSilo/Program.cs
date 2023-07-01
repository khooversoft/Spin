using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.State;
using Toolbox.Extensions;

SpinClusterOption option = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets("SpinSilo")
    .Build()
    .Bind<SpinClusterOption>()
    .Verify();

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering()
            .ConfigureLogging(logging => logging.AddConsole())
            .AddDatalakeGrainStorage(option);
        //.AddAzureBlobGrainStorage("User", config =>
        //{
        //    config.ConfigureBlobServiceClient(option.StorageAccountConnectionString);
        //});
    })
    .UseConsoleLifetime()
    .ConfigureLogging(config =>
    {
        config.AddApplicationInsights(
            configureTelemetryConfiguration: (config) => config.ConnectionString = option.ApplicationInsightsConnectionString,
            configureApplicationInsightsLoggerOptions: (options) => { }
        );
    })
    .ConfigureServices(services =>
    {
        services.AddSpinCluster(option);
    });

using IHost host = builder.Build();

await host.UseSpinCluster();

Console.WriteLine($"Spin Silo - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

await host.RunAsync();