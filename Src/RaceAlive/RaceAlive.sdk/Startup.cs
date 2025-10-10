using Microsoft.Extensions.DependencyInjection;

namespace RaceAlive.sdk;

public static class Startup
{
    public static IServiceCollection AddRaceAlive(this IServiceCollection serviceDescriptors)
    {
        serviceDescriptors.AddSingleton<MarathonScheduleClient>();
        return serviceDescriptors;
    }
}
