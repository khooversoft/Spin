using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TicketShareWeb.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

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
})
// Microsoft personal accounts (Outlook/Hotmail/Xbox) via Microsoft Identity Platform (consumers tenant)
.AddOpenIdConnect("Microsoft", options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = "https://login.microsoftonline.com/consumers/v2.0";
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
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
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
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
})
//// Apple ID OpenID (via aspnet-contrib provider)
//.AddApple("Apple", options =>
//{
//    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.ClientId = builder.Configuration["Authentication:Apple:ClientId"]!;
//    options.TeamId = builder.Configuration["Authentication:Apple:TeamId"]!;
//    options.KeyId = builder.Configuration["Authentication:Apple:KeyId"]!;
//    options.CallbackPath = "/signin-apple";
//    options.SaveTokens = true;

//    // Name + email scopes; email may be returned once on first consent
//    options.Scope.Add("name");
//    options.Scope.Add("email");

//    // Load the Apple private key used to create the client secret JWT
//    //var privateKeyPath = builder.Configuration["Authentication:Apple:PrivateKeyPath"];
//    //if (!string.IsNullOrWhiteSpace(privateKeyPath))
//    //{
//    //    options.UsePrivateKey(keyId => File.OpenRead(Path.Combine(builder.Environment.ContentRootPath, privateKeyPath!)));
//    //}
//})
;

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

app.UseRouting();

// AuthN/AuthZ
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery middleware required for Razor Components endpoints
app.UseAntiforgery();

// External login challenge endpoint: /signin/{scheme}
app.MapGet("/signin/{scheme}", (string scheme, HttpContext ctx) =>
{
    var supported = new[] { "Microsoft", "Google", "Apple" };
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
