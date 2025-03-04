using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public static class TicketStartup
{
    public static IServiceCollection AddTicket(this IServiceCollection services, TicketOption ticketOption)
    {
        ticketOption.NotNull();
        ticketOption.Validate().ThrowOnError("StorageOption is invalid");

        services.AddSingleton(ticketOption);
        services.AddSingleton(_ => new MonitorRate(TimeSpan.FromSeconds(1), 3, 5));

        services.AddSingleton<TicketDataClient>();
        services.AddSingleton<TicketDataManager>();

        services.AddHttpClient<TicketEventClient>();
        services.AddHttpClient<TicketClassificationClient>();
        services.AddHttpClient<TicketAttractionClient>();

        return services;
    }
}
