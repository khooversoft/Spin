using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RaceAliveWeb.Application;
using RaceAliveWeb.Components;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

Console.WriteLine($"Starting {AppProgram.ServiceName} ver {AppProgram.ServiceVersion} ...");
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Configure to use PORT environment variable for Azure Container Apps
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    Console.WriteLine($"Using PORT environment variable: {port}");
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Use AppRegistration's client credentials for Key Vault access

//builder.Logging.AddFilter(_ => true);


// OpenTelemetry + Azure Monitor (App Insights)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(serviceName: AppProgram.ServiceName, serviceVersion: AppProgram.ServiceVersion)
        .AddAttributes(new[]
        {
            new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
        }))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation())
    .UseAzureMonitor(o =>
    {
        // Uses the provided Application Insights connection string
        o.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });

// Optional: send .NET logs via OpenTelemetry as well
builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeScopes = true;
    o.IncludeFormattedMessage = true;
});

var appRegOption = builder.Configuration.GetSection("AppRegistration").Get<AppRegistrationOption>().NotNull("AppRegistration not in appsettings.json");
appRegOption.Validate().ThrowOnError();

TokenCredential credential = appRegOption.ClientSecret switch
{
    string => new ClientSecretCredential(appRegOption.TenantId, appRegOption.ClientId, appRegOption.ClientSecret).Action(_ => Console.WriteLine("Using ClientSecretCredential")),
    null => new DefaultAzureCredential().Action(_ => Console.WriteLine("Using default credential")),
};

// Add Azure Key Vault configuration (after defaults so KV overrides others)
builder.Configuration.AddAzureKeyVault(new Uri(appRegOption.VaultUri), credential);

var authOption = builder.Configuration.GetSection("Authentication").Get<AuthenticationOption>().NotNull("AppRegistration not in appsettings.json");
authOption.Validate().ThrowOnError();

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/";      // When a page requires auth, send back to home to choose a provider
    options.LogoutPath = "/signout";
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // <-- ensure secure cookies behind TLS-terminating proxy
})
// Microsoft personal accounts (Outlook/Hotmail/Xbox) via Microsoft Identity Platform (consumers tenant)
.AddOpenIdConnect("Microsoft", options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = "https://login.microsoftonline.com/consumers/v2.0";
    options.ClientId = authOption.Microsoft.ClientId; // builder.Configuration["Authentication:Microsoft:ClientId"].Be(authOption.Microsoft.ClientId);
    options.ClientSecret = authOption.Microsoft.ClientSecret; // builder.Configuration["Authentication:Microsoft:ClientSecret"].Be(authOption.Microsoft.ClientSecret);
    options.CallbackPath = "/signin-oidc-microsoft";
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;

    // Force account picker so the user can enter/select an email every time
    options.Prompt = "select_account";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.GetClaimsFromUserInfoEndpoint = true;
})
// Google OpenID Connect
.AddOpenIdConnect("Google", options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = "https://accounts.google.com";
    options.ClientId = authOption.Google.ClientId; // builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = authOption.Google.ClientSecret; // builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.CallbackPath = "/signin-oidc-google";
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;

    // Force account picker so the user can enter/select an email every time
    options.Prompt = "select_account";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

var app = builder.Build();

// Process X-Forwarded-* from Azure Container Apps ingress BEFORE anything that uses scheme/host
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
// Accept from any proxy (managed env); if you know the proxy IPs, register them instead.
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

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

// AuthN/AuthZ
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery middleware required for Razor Components endpoints
app.UseAntiforgery();

// External login challenge endpoint: /signin/{scheme}
app.MapGet("/signin/{scheme}", (string scheme, HttpContext ctx) =>
{
    var supported = new[] { "Microsoft", "Google" };
    if (!supported.Contains(scheme, StringComparer.OrdinalIgnoreCase))
    {
        return Results.NotFound($"Authentication scheme '{scheme}' is not configured.");
    }

    var props = new AuthenticationProperties
    {
        RedirectUri = "/"
    };

    return Results.Challenge(props, new[] { scheme });
})
.AllowAnonymous();

// Local sign-out (clears the app cookie). Remote sign-out is optional and provider-specific.
app.MapPost("/signout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
})
.AllowAnonymous()
.DisableAntiforgery();

// Blazor root
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
