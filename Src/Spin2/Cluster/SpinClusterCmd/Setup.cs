using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Client;
using SpinClusterCmd.Application;
using SpinClusterCmd.Commands;

namespace SpinClusterCmd;

internal static class Setup
{
    public static IServiceCollection AddApplication(this IServiceCollection services, CmdOption option)
    {
        services.AddSingleton(option);
        services.AddSingleton<UserCommand>();

        services.AddHttpClient<SpinClusterClient>((services, httpClient) =>
        {
            var option = services.GetRequiredService<CmdOption>();

            httpClient.BaseAddress = new Uri(option.ClusterApi);
        });

        return services;
    }
}
