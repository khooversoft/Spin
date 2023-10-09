using LoanContract.sdk.Contract;
using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Application;

namespace LoanContract.sdk.Application;

public static class LoanContractSetup
{
    public static IServiceCollection AddLoanContract(this IServiceCollection services)
    {
        services.AddClusterHttpClient<LoanContractManager>();

        return services;
    }
}
