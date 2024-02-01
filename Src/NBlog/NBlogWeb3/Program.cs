using System.Reflection;
using NBlog.sdk;
using NBlogWeb3.Application;
using NBlogWeb3.Components;
using NBlogWeb3.Models;
using Toolbox.Extensions;
using static System.Net.Mime.MediaTypeNames;


Console.WriteLine($"NBlog web Server - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();


AppOption option = Host.CreateApplicationBuilder(args)
    .Build()
    .Func(x => x.Services.GetRequiredService<IConfiguration>().Bind<AppOption>());

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    })
    .AddDebug()
    .AddFilter(x => x >= LogLevel.Warning);

if (option.AppInsightsConnectionString != null)
{
    builder.Logging.AddApplicationInsights(
        configureTelemetryConfiguration: (config) => config.ConnectionString = option.AppInsightsConnectionString,
        configureApplicationInsightsLoggerOptions: (options) => { }
    );
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Host.UseOrleans((context, silo) =>
{
    silo.UseLocalhostClustering();
    silo.AddBlogCluster(context);
    silo.AddDatalakeGrainStorage();
});

builder.Services.AddHealthChecks();
builder.Services.AddScoped<LeftButtonStateService>();

var app = builder.Build();
//app.UseStatusCodePagesWithReExecute("/summary/article/home");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStatusCodePagesWithRedirects("/");
app.UseRouting();
app.UseAntiforgery();
app.MapBlogApi();
app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
