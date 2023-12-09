using Microsoft.Extensions.DependencyInjection;
using Toolbox.Metrics;
using Toolbox.Tools;

namespace Toolbox.Azure.Extensions;

public static class MetricStartup
{
    public static IServiceCollection AddMetricApplicationInsight(this IServiceCollection services)
    {
        services.NotNull();

        services.AddSingleton<IMetric, AzureAppInsightsMetric>();
        return services;
    }
}
