using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;

namespace SoftBank.sdk.Application;

public static class SoftBankStartup
{
    public static ISiloBuilder AddSoftBank(this ISiloBuilder builder)
    {
        builder.NotNull();

        builder.ConfigureServices(services => services.AddSoftBank());
        return builder;
    }

    public static IServiceCollection AddSoftBank(this IServiceCollection services)
    {
        services.NotNull();

        services.AddSingleton<IValidator<LedgerItem>>(LedgerTypeValidator.Validator);
        services.AddSingleton<IValidator<AccountDetail>>(AccountDetailValidator.Validator);
        services.AddSingleton<IValidator<BlockAcl>>(BlockAclValidator.Validator);

        services.AddSingleton<ISign, SignProxy>();
        services.AddSingleton<ISignValidate, SignValidationProxy>();
        services.AddSingleton<SoftBankFactory>();

        services.AddTransient<AccountDetailImpl>();
        services.AddTransient<AclImpl>();
        services.AddTransient<LedgerItemImpl>();

        return services;
    }
}
