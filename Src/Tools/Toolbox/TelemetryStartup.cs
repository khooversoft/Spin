using Microsoft.Extensions.DependencyInjection;
using Toolbox.Telemetry;
using Toolbox.Tools;

namespace Toolbox;

public static class TelemetryStartup
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, Action<TelemetryOption>? options = null)
    {
        services.NotNull();

        var option = new TelemetryOption();
        options?.Invoke(option);

        services.AddSingleton<TelemetryOption>(option);
        services.AddSingleton<TelemetryCollector>();
        services.AddSingleton<ITelemetry, TelemetryFactory>();

        return services;
    }
}
