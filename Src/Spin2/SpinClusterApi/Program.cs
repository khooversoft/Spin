using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Models;
using SoftBank.sdk.Application;
using SpinCluster.sdk.Application;
using SpinClusterApi.Application;
using Toolbox.Extensions;

[assembly: InternalsVisibleTo("SpinClusterApi.test")]

Console.WriteLine($"Spin Cluster API - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

ApiOption option = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build()
    .Bind<ApiOption>();

IHostBuilder builder2 = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.AddDebug();
        logging.AddApplicationInsights(
                configureTelemetryConfiguration: (config) => config.ConnectionString = option.AppInsightsConnectionString,
                configureApplicationInsightsLoggerOptions: (options) => { }
            );
    })
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
        silo.AddSpinCluster();
        //silo.AddSoftBank();

    })
    .UseConsoleLifetime()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices(services =>
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

var h = builder2.Build();
await h.RunAsync();

//var builder = WebApplication.CreateBuilder(args);

////ApiOption option = new ConfigurationBuilder()
////    .AddJsonFile("appsettings.json")
////    .Build()
////    .Bind<ApiOption>();

//builder.Logging
//    .AddConsole()
//    .AddDebug()
//    .AddApplicationInsights(
//        configureTelemetryConfiguration: (config) => config.ConnectionString = option.AppInsightsConnectionString,
//        configureApplicationInsightsLoggerOptions: (options) => { }
//    );

////builder.Host.UseOrleansClient(silobuilder =>
////{
////    silobuilder.UseLocalhostClustering();%
////});

//builder.Host.UseOrleans(silo =>
//{
//    silo.UseLocalhostClustering();
//    silo.AddSpinCluster();
//    //silo.AddSoftBank();

//})
//    .UseConsoleLifetime();

//// Add services to the container.
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddSpinApi();
//builder.Services.AddSpinApiInternal();
//builder.Services.AddHealthChecks();

//WebApplication app = builder.Build();

//// Configure the HTTP request pipeline.
//if (option.UseSwagger)
//{
//    Console.WriteLine("Using swagger");
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.MapSpinApi();
//app.MapSpinApiInternal();

//option.IpAddress.Split(';').ForEach(x => app.Urls.Add(x));

//app.MapHealthChecks("/_health");

//app.WaitForSpinSilo();

//Console.WriteLine("Running");
//Console.WriteLine();

//await app.RunAsync();
