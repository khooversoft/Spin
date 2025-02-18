using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;

namespace TicketShare.sdk;

public static class TicketShareStartup
{
    public static IServiceCollection AddTicketShare(this IServiceCollection services)
    {
        services.AddGraphExtensions();
        services.AddSingleton<AccountClient>();
        services.AddSingleton<ChannelManager>();
        services.AddScoped<UserAccountContext>();

        services.AddSingleton<TicketGroupClient>();
        services.AddScoped<TicketGroupManager>();

        services.AddChannel<EmailMessage>();
        services.AddScoped<VerifyEmail>();
        services.AddHostedService<EmailSenderHost>();

        services.AddChannel<ChannelMessage>();
        services.AddSingleton<ChannelManager>();

        services.AddScoped<MessageSender>();
        services.AddHostedService<MessageSenderHost>();

        return services;
    }

    public static IServiceCollection AddChannel<T>(this IServiceCollection services)
    {
        services.AddSingleton<Channel<T>>(service =>
        {
            var logger = service.GetRequiredService<ILoggerFactory>().CreateLogger("TicketShareStartup");

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

