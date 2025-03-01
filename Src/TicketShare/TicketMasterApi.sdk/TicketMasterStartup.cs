using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public static class TicketMasterStartup
{
    public static IServiceCollection AddTicketMaster(this IServiceCollection services, IConfigurationSection configuration)
    {
        configuration.NotNull();

        TicketMasterOption ticketMasterOption = configuration.Get<TicketMasterOption>().NotNull("Failed to parse configuration for TicketMasterOption");
        ticketMasterOption.Validate().Assert(x => x.IsOk(), option => $"StorageOption is invalid, errors={option.Error}");

        services.AddSingleton(ticketMasterOption);
        services.AddHttpClient<TicketMasterEventClient>();
        services.AddHttpClient<TicketMasterClassificationClient>();
        services.AddHttpClient<TicketMasterAttractionClient>();
        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return services;
    }
}
