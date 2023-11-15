using System.Reflection;
using System.Runtime.CompilerServices;
using SoftBank.sdk.Application;
using SpinCluster.sdk.Application;
using SpinClusterApi.Application;
using Toolbox.Azure.Extensions;
using Toolbox.Extensions;

[assembly: InternalsVisibleTo("SpinClusterApi.test")]

Console.WriteLine($"Spin Cluster API - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

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
        logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
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
        silo.AddSoftBank();
    })
    .UseConsoleLifetime()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices((context, services) =>
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddApplicationInsightsTelemetry(config => config.EnableAdaptiveSampling = false);

            services.AddSpinApi();
            services.AddMetricApplicationInsight();
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
                config.MapSoftBank();
            });
        });
    });

IHost host = builder.Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
ILogger<Program> logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

var msg = new[]
{
    $"Running environment={config["environment"]}",
    "",
    "Starting server"
}.Join(Environment.NewLine);

logger.LogInformation(msg);
Console.WriteLine(msg);

Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine();
    Console.WriteLine("Shutting silo down...");
};

host.Run();
