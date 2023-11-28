using Microsoft.Extensions.DependencyInjection;

namespace SpinAgent.sdk;

public static class SpinAgentStartup
{
    public static IServiceCollection AddSpinAgent(this IServiceCollection services)
    {
        services.AddSingleton<RunSmartC>();
        services.AddTransient<LookForWorkActivity>();

        services.AddTransient<AgentSession>((services) =>
        {
            var option = services.GetRequiredService<AgentOption>();
            return ActivatorUtilities.CreateInstance<AgentSession>(services, option);
        });

        return services;
    }
}
