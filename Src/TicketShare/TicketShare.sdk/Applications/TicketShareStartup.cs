using Microsoft.Extensions.DependencyInjection;
using Toolbox.Identity;

namespace TicketShare.sdk;

public static class TicketShareStartup
{
    public static IServiceCollection AddTicketShare(this IServiceCollection service)
    {
        service.AddIdentity();
        service.AddSingleton<AccountClient>();
        service.AddSingleton<TicketGroupClient>();
        return service;
    }
}
