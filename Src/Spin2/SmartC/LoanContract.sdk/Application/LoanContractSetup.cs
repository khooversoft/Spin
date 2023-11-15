using LoanContract.sdk.Activities;
using LoanContract.sdk.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace LoanContract.sdk.Application;

public static class LoanContractSetup
{
    public static IServiceCollection AddLoanContract(this IServiceCollection services)
    {
        services.AddTransient<LoanContractManager>();
        services.AddTransient<CreateContractActivity>();
        services.AddTransient<PaymentActivity>();

        return services;
    }
}
