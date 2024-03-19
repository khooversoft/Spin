using System.Reflection;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using TicketShare.sdk;
using TicketShareWeb.Components;
using TicketShareWeb.Components.Account;
using Toolbox.Identity;
using Toolbox.Identity.Store;
using Toolbox.Orleans;

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

builder.Services.AddIdentityCore<PrincipalIdentity>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IUserStore<PrincipalIdentity>, UserStore>();

//builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Host.UseOrleans((context, silo) =>
{
    silo.UseLocalhostClustering();
    silo.AddTickShareCluster(context);
});

var app = builder.Build();

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

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
