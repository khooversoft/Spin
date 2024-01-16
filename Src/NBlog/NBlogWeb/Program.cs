using System.Reflection;
using NBlog.sdk;
using NBlog.sdk.State;
using NBlogWeb.Application;
using NBlogWeb.Components;
using Toolbox.Extensions;

Console.WriteLine($"NBlog web Server - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

AppOption option = Host.CreateApplicationBuilder(args)
    .Build()
    .Func(x => x.Services.GetRequiredService<IConfiguration>().Bind<AppOption>());

var builder = WebApplication.CreateBuilder(args);

if (option.UserSecrets != null)
{
    builder.Configuration.AddUserSecrets(option.UserSecrets);
}

//var b = builder.Build();
//var bb = b.Services.GetRequiredService<IConfiguration>().Bind<StorageOption>();

//Console.WriteLine($"Secret: {bb.Storage.ToString()}");

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

var app = builder.Build();
app.UseStatusCodePagesWithReExecute("/NotFound/{0}");

//var bb = app.Services.GetRequiredService<IConfiguration>().Bind<StorageOption>();
//Console.WriteLine($"Secret: {bb.Storage.ToString()}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
