using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using TicketShare.sdk;
using TicketShareWeb.Components;
using Toolbox.Azure;
using Toolbox.Graph;
using Toolbox.Identity;
//using Toolbox.Types;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets("aspnet-TicketShareWeb-e9076773-e3de-4c20-8260-df0e9c390006");

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    //.AddCookie(options =>
    //{
    //    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    //    options.SlidingExpiration = true;
    //    options.AccessDeniedPath = "/Forbidden/";
    //})
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services
    .AddIdentityCore<PrincipalIdentity>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddDatalakeFileStore(builder.Configuration.GetSection("Storage"))
    .AddGraphEngine()
    .AddTicketShare();

//builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
//{
//    options.Events = new OpenIdConnectEvents
//    {
//        OnTokenValidated = async context =>
//        {
//            var identityPrincipalManager = context.HttpContext.RequestServices.GetRequiredService<IdentityPrincipalManager>();
//            string principalId = context?.Principal?.Identity?.Name ?? throw new Exception("PrincipalId is missing");

//            var findResult = await identityPrincipalManager.GetPrincipalId(principalId);
//            if (findResult.IsNotFound())
//            {
//                context.Response.Redirect("/Account/CreateLogon");
//                context.HandleResponse(); // This prevents the request from proceeding further                                          
//            }
//        }
//    };
//});

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();

builder.Services
    .AddControllersWithViews()
    .AddMicrosoftIdentityUI();

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

app.UseAuthentication();
app.UseAuthorization();

app.Run();
