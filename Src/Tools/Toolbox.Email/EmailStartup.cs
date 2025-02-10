using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Email;

public static class EmailStartup
{
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfigurationSection configure)
    {
        services.Configure<EmailOption>(configure);
        services.AddSingleton<IEmailSender, MailKitProvider>();
        return services;
    }

    public static IServiceCollection AddEmail(this IServiceCollection services, Action<EmailOption> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IEmailSender, MailKitProvider>();
        return services;
    }
}
