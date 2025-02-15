using System.Security.Claims;
using System.Threading.Channels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;

namespace TicketShare.sdk;

public static class TicketShareStartup
{
    public static IServiceCollection AddTicketShare(this IServiceCollection services)
    {
        services.AddGraphExtensions();
        services.AddSingleton<AccountClient>();
        services.AddScoped<UserAccountManager>();
        services.AddScoped<AuthenticationAccess>();
        services.AddSingleton<ChannelManager>();

        services.AddSingleton<TicketGroupClient>();
        services.AddScoped<TicketGroupManager>();

        services.AddChannel<EmailMessage>();
        services.AddScoped<VerifyEmail>();
        services.AddHostedService<EmailSenderHost>();

        services.AddChannel<ChannelMessage>();
        services.AddScoped<MessageSender>();
        services.AddHostedService<MessageSenderHost>();
        services.AddSingleton<ChannelManager>();

        return services;
    }

    public static IServiceCollection AddChannel<T>(this IServiceCollection services)
    {
        services.AddSingleton<Channel<T>>(service =>
        {
            var logger = service.GetRequiredService<ILogger<EmailSenderHost>>();

            var bounded = new BoundedChannelOptions(1000)
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            };

            Channel<T> channel = Channel.CreateBounded<T>(
                bounded,
                x => logger.LogError("Channel dropped, message={message}", x)
            );

            return channel;
        });

        return services;
    }

}

