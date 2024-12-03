using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using TicketShare.sdk;
using TicketShareWeb.Application;
using TicketShareWeb.Components;
using TicketShareWeb.Components.Account;
using Toolbox.Azure;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddCascadingAuthenticationState();
//builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddMicrosoftAccount(opt =>
    {
        opt.ClientId = builder.Configuration[TsConstants.AzureAd_ClientId].NotEmpty();
        opt.ClientSecret = builder.Configuration[TsConstants.AzureAd_ClientSecret].NotEmpty();

        // Adding the prompt parameter
        opt.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                return Task.CompletedTask;
            }
        };
    })
    .AddIdentityCookies();

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddIdentityCore<PrincipalIdentity>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddSignInManager()
//    .AddDefaultTokenProviders();

builder.Services.AddTransient<IUserStore<PrincipalIdentity>, IdentityUserStore>();

builder.Services
    .AddIdentityCore<PrincipalIdentity>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddDatalakeFileStore(builder.Configuration.GetSection("Storage"))
    .AddGraphEngine()
    .AddTicketShare()
    .AddScoped<UserAccountManager>();

builder.Services.AddSingleton<IEmailSender<PrincipalIdentity>, IdentityNoOpEmailSender>();

var app = builder.Build();

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

app.Run();
