using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Directory.Models;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Application;

public static class Setup
{
    public static IServiceCollection AddSpinCluster(this IServiceCollection services)
    {
        services.AddSingleton<Validator<UserPrincipal>>(UserPrincipalValidator.Validator);

        return services;
    }
}
