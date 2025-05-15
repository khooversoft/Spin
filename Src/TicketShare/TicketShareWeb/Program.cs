using System.Reflection;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using TicketApi.sdk;
using TicketShare.sdk;
using TicketShare.sdk.Identity;
using TicketShareWeb.Application;
using TicketShareWeb.Components;
using TicketShareWeb.Components.Account;
using Toolbox.Azure;
using Toolbox.Email;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

const string _appVersion = "TicketShareWeb - Version: 0.8.1";
Console.WriteLine(_appVersion);

var builder = WebApplication.CreateBuilder(args);
bool enableHttpLogging = builder.Configuration.GetValue<bool>("EnableHttpLogging", false);

Console.WriteLine($"Running in developer mode={(builder.Environment.IsDevelopment() ? "yes" : "no")}");

//builder.Logging.AddApplicationInsights(
//        configureTelemetryConfiguration: (config) =>
//            {
//                string instrumentKey = builder.Configuration["InstrumentationKey"].NotEmpty();
//                config.ConnectionString = instrumentKey;
//            },
//        configureApplicationInsightsLoggerOptions: (options) => { }
//    );

builder.Logging
    .AddConsole()
    .AddFilter(x => true);

if (enableHttpLogging || true)
{
    builder.Services.AddHttpLogging(config =>
    {
        config.RequestHeaders.Add("X-Forwarded-Proto");
        config.RequestHeaders.Add("X-Forwarded-For");
        config.RequestHeaders.Add("X-Forwarded-Host");
        config.RequestHeaders.Add("X-Forwarded-Client-Cert");
    });
}

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

//builder.Services.Configure<ForwardedHeadersOptions>(options =>
//{
//    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
//});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddMicrosoftAccount(opt =>
    {
        opt.ClientId = builder.Configuration[TsConstants.AzureAd_ClientId].NotEmpty($"{TsConstants.AzureAd_ClientId} is required");
        opt.ClientSecret = builder.Configuration[TsConstants.AzureAd_ClientSecret].NotEmpty($"{TsConstants.AzureAd_ClientSecret} is required");
        opt.CallbackPath = builder.Configuration[TsConstants.AzureAd_CallbackPath].NotEmpty($"{TsConstants.AzureAd_CallbackPath} is required");
        opt.SaveTokens = true;

        // Adding the prompt parameter
        opt.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                var str = $"Console: RedirectUri='{context.RedirectUri}'";
                Console.WriteLine(str);

                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                return Task.CompletedTask;
            }
        };
    }).AddIdentityCookies();

builder.Services.AddScoped<IUserStore<PrincipalIdentity>, IdentityUserStoreHandler>();

builder.Services
    .AddIdentityCore<PrincipalIdentity>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddDatalakeFileStore(builder.Configuration.Get<DatalakeOption>("Storage", DatalakeOption.Validator).NotNull())
    .AddGraphEngine()
    .AddTicketShare()
    .AddTicketData()
    .AddEmail(builder.Configuration.Get<EmailOption>("email", EmailOption.Validator).NotNull())
    .AddScoped<AskPanel>()
    .AddScoped<ApplicationNavigation>()
    .AddSingleton<TestDisposable>();


///////////////////////////////////////////////////////////////////////////////

WebApplication app = builder.Build();

///////////////////////////////////////////////////////////////////////////////

if (enableHttpLogging || true) app.UseHttpLogging();
//app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseMigrationsEndPoint();
}
else
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

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation(
    "{appVersion}, Environment={environmentName}: AssemblyVersion={assemblyVersion}",
    _appVersion,
    app.Environment.EnvironmentName,
    Assembly.GetExecutingAssembly().GetName().Version
    );

var runOption = await app.Services.StartGraphEngine();

// Example in Program.cs
app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is stopping...***");
    logger.LogInformation("Application is stopping...*!*");
    // Add timing or additional diagnostics here
});

if (runOption.IsOk())
{
    logger.LogInformation("Starting app");
    await app.RunAsync();
}


public class TestDisposable : IDisposable, IAsyncDisposable
{
    private readonly ILogger<TestDisposable> _logger;

    public TestDisposable(ILogger<TestDisposable> logger)
    {
        _logger = logger;
        _logger.LogInformation("TestDisposable created");
    }

    public void Dispose()
    {
        Console.WriteLine("TestDisposable CALLED**");
        _logger.LogInformation("TestDisposable CALLED");
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("DisposeAsync CALLED**");
        _logger.LogInformation("DisposeAsync CALLED");
        return ValueTask.CompletedTask;
    }
}