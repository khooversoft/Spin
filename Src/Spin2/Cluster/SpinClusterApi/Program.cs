using System.Diagnostics;
using System.Reflection;
using Orleans.Hosting;
using SpinClusterApi.Application;
using Toolbox.Extensions;


Console.WriteLine($"Spin Cluster API - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var builder = WebApplication.CreateBuilder(args);

ApiOption option = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build()
    .Bind<ApiOption>();

builder.Host.UseOrleansClient(silobuilder =>
{
    silobuilder.UseLocalhostClustering();
});

builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: (config) => config.ConnectionString = option.AppInsightsConnectionString,
    configureApplicationInsightsLoggerOptions: (options) => { }
);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSpinApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (option.UseSwagger)
{
    Console.WriteLine("Using swagger");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapSpinApi();
option.Urls.Split(';').ForEach(x => app.Urls.Add(x));

app.WaitForSpinSilo();

Console.WriteLine("Running");
Console.WriteLine();

app.Run();
