using Azure.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Toolbox.Azure.Identity;
using Toolbox.Tools;
using Microsoft.AspNetCore.Identity;
using TicketShare.sdk;
using Toolbox.Orleans;
using Microsoft.AspNetCore.Authentication.OAuth;
using TicketShareWeb.Components.Account;

namespace TicketShareWeb.Application;

public static class Setup
{
    public static WebApplicationBuilder AddTicketShareAuthentication(this WebApplicationBuilder builder)
    {
        builder.NotNull();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddScoped<UserProfileEdit>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddMicrosoftAccount(opt =>
            {
                opt.ClientId = builder.Configuration[TsConstants.Authentication.ClientId].NotEmpty();
                opt.ClientSecret = builder.Configuration[TsConstants.Authentication.ClientSecret].NotEmpty();

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

        builder.Services.AddIdentityCore<PrincipalIdentity>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        
        builder.Services.AddTransient<IUserStore<PrincipalIdentity>, UserStore>();
        return builder;
    }
}
