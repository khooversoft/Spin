using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Models;
using Orleans.Runtime;
using SoftBank.sdk.Application;
using SpinCluster.sdk.Application;
using SpinClusterApi.Application;
using Toolbox.Extensions;

[assembly: InternalsVisibleTo("SpinClusterApi.test")]

Console.WriteLine($"Spin Cluster API - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

//ApiOption option = new ConfigurationBuilder()
//    .AddJsonFile("appsettings.json")
//    .Build()
//    .Bind<ApiOption
//    

ApiOption option = Host.CreateApplicationBuilder(args)
    .Build()
    .Func(x => x.Services.GetRequiredService<IConfiguration>().Bind<ApiOption>());

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        if (option.UserSecrets.IsNotEmpty()) builder.AddUserSecrets(option.UserSecrets);
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.AddDebug();
        logging.AddApplicationInsights(
                configureTelemetryConfiguration: (config) => config.ConnectionString = option.AppInsightsConnectionString,
                configureApplicationInsightsLoggerOptions: (options) => { }
            );
    })
    .UseOrleans((context, silo) =>
    {
        silo.UseLocalhostClustering();
        silo.AddSpinCluster(context);
        //silo.AddSoftBank();

    })
    .UseConsoleLifetime()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices((context, services) =>
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddSpinApi();
            services.AddSpinApiInternal();
            services.AddHealthChecks();
        });

        webBuilder.UseUrls(option.IpAddress.Split(';').ToArray());

        webBuilder.Configure((context, app) =>
        {
            // Configure the HTTP request pipeline.
            if (option.UseSwagger)
            {
                Console.WriteLine("Using swagger");
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(config =>
            {
                config.MapHealthChecks("/_health");

                config.MapSpinApi();
                config.MapSpinApiInternal();
            });
            //app.MapHealthChecks("/_health");

            //app.MapSpinApi();
            //app.MapSpinApiInternal();

            //option.IpAddress.Split(';').ForEach(x => app.Urls.Add(x));

            //app.MapHealthChecks("/_health");
        });
    });

IHost host = builder.Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
ILogger<Program> logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

var msg = new[]
{
    $"Spin Cluster API - Version {Assembly.GetExecutingAssembly().GetName().Version}",
    $"Running, environment={config["environment"]}",
}.Join(Environment.NewLine);

logger.LogInformation(msg);

host.Run();

