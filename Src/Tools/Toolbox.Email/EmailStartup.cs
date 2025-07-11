using Microsoft.Extensions.DependencyInjection;
using Toolbox.Types;

namespace Toolbox.Email;

public static class EmailStartup
{
    public static IServiceCollection AddEmail(this IServiceCollection services, EmailOption emailOption)
    {
        emailOption.Validate().ThrowOnError("EmailOption is invalid");

        services.AddSingleton<EmailOption>(emailOption);
        services.AddSingleton<IEmailWriter, MailKitProvider>();
        return services;
    }
}
