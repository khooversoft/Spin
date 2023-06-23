using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Directory;
using SpinCluster.sdk.Lease;
using SpinCluster.sdk.Storage;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Application;

public static class Setup
{
    public static IServiceCollection AddSpinCluster(this IServiceCollection services, SpinClusterOption option)
    {
        services.AddSingleton(option);

        services.AddSingleton<Validator<UserPrincipal>>(UserPrincipalValidator.Validator);
        services.AddSingleton<Validator<PrincipalKey>>(PrincipalKeyValidator.Validator);
        services.AddSingleton<Validator<StorageBlob>>(StorageBlobValidator.Validator);

        return services;
    }
}
