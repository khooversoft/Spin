using Bank.sdk.Service;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Bank.sdk;

public static class Extensions
{
    public static IServiceCollection AddBankHost(this IServiceCollection service, BankOption bankOption)
    {
        service.VerifyNotNull(nameof(service));
        service.VerifyNotNull(nameof(bankOption));

        service.AddSingleton(bankOption);

        service.AddSingleton<BankHost>();
        service.AddHostedService<BankHost>(x => x.GetRequiredService<BankHost>());

        return service;
    }
}
