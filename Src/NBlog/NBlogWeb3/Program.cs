using System.Reflection;
using NBlogWeb3.Application;
using NBlogWeb3.Components;
using Toolbox.Extensions;
using NBlog.sdk;


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
    .AddDebug();

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

var app = builder.Build();
app.UseStatusCodePagesWithReExecute("/NotFound/{0}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHealthChecks("/health");
app.MapBlogApi();

app.Run();
