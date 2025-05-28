using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public static class TicketStartup
{
    //public static IServiceCollection AddTicketData(this IServiceCollection services)
    //{
    //    //services.AddSingleton<TicketDataClient>();
    //    services.TryAddSingleton<IMemoryCache, MemoryCache>();

    //    return services;
    //}

    public static IServiceCollection AddTicketApi(this IServiceCollection services, TicketOption ticketOption)
    {
        ticketOption.NotNull();
        ticketOption.Validate().ThrowOnError("StorageOption is invalid");

        services.AddSingleton(ticketOption);
        services.AddSingleton(_ => new MonitorRate(TimeSpan.FromSeconds(1), 3, 5));
        services.AddSingleton<TicketMasterClient>();
        services.AddSingleton<TicketSearchClient>();

        services.AddHttpClient<TicketEventClient>();
        services.AddHttpClient<TicketClassificationClient>();
        services.AddHttpClient<TicketAttractionClient>();

        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return services;
    }
}
