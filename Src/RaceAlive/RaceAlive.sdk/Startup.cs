using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
