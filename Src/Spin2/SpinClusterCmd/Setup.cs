using Microsoft.Extensions.DependencyInjection;
using SpinClusterCmd.Application;

namespace SpinClusterCmd;

internal static class Setup
{
    public static IServiceCollection AddApplication(this IServiceCollection services, CmdOption option)
    {
        services.AddSingleton(option);

        //services.AddSingleton<DirectoryCommand>();

        //services.AddHttpClient<DirectoryClient>((services, httpClient) =>
        //{
        //    var option = services.GetRequiredService<CmdOption>();
        //    httpClient.BaseAddress = new Uri(option.ClusterApi);
        //});

        return services;
    }
}
