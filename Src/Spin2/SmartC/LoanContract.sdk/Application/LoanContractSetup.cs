using LoanContract.sdk.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace LoanContract.sdk.Application;

public static class LoanContractSetup
{
    public static IServiceCollection AddLoanContract(this IServiceCollection services)
    {
        services.AddSingleton<LoanContractManager>();

        return services;
    }
}
