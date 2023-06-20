using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using SpinSilo.Application;
using Toolbox.Extensions;

SiloOption option = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets("SpinSilo")
    .Build()
    .Bind<SiloOption>();

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering()
            .ConfigureLogging(logging => logging.AddConsole())
            .AddAzureBlobGrainStorage("User", config =>
            {
                config.ConfigureBlobServiceClient(option.StorageAccountConnectionString);
            });
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
        services.AddSpinCluster();
    });

using IHost host = builder.Build();

Console.WriteLine($"Spin Silo - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

await host.RunAsync();