using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using TicketShareWeb.Components;
using TicketShareWeb.Components.Account;
using TicketShareWeb.Data;
using Toolbox.Orleans;
using Toolbox.Identity;
using System.Reflection;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using TicketShareWeb.Application;
using Toolbox.Tools;
using TicketShare.sdk;

Console.WriteLine($"Ticket Share Web Server - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.AddApplicationConfiguration();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddMicrosoftAccount(opt =>
    {
        opt.ClientId = builder.Configuration[TsConstants.Authentication.ClientId]!;
        opt.ClientSecret = builder.Configuration[TsConstants.Authentication.ClientSecret]!;

        // Adding the prompt parameter
        opt.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                return Task.CompletedTask;
            }
        };
    }).AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Host.UseOrleans((context, silo) =>
{
    silo.UseLocalhostClustering();
    silo.AddTickShareCluster(context);
    silo.AddDatalakeGrainStorage();
    silo.AddIdentityActor(context);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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

app.Run();
