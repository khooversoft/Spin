using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox;
using Toolbox.Data;
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
        services.AddSingleton<IconCollectionService>();

        services.AddHttpClient<TmEventClient>();
        services.AddHttpClient<TmClassificationClient>();
        services.AddHttpClient<TmAttractionClient>();

        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        services.AddDataClient<ClassificationRecord>(builder =>
        {
            builder.AddMemoryCache();
            builder.AddFileStoreCache();
            builder.AddProvider<TmClassificationHandler>();
        });

        services.AddDataClient<EventCollectionRecord>(builder =>
        {
            builder.AddMemoryCache();
            builder.AddFileStoreCache();
            builder.AddProvider<TmEventHandler>();
        });

        services.AddDataClient<AttractionCollectionRecord>(builder =>
        {
            builder.AddMemoryCache();
            builder.AddFileStoreCache();
            builder.AddProvider<TmAttractionHandler>();
        });

        return services;
    }
}
