using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Application;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Application;

public static class SoftBankStartup
{
    public static ISiloBuilder AddSoftBank(this ISiloBuilder builder)
    {
        builder.NotNull();

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IValidator<LedgerItem>>(LedgerTypeValidator.Validator);
            services.AddSingleton<IValidator<AccountDetail>>(AccountDetailValidator.Validator);

            services.AddSingleton<ISign, SignProxy>();
            services.AddSingleton<ISignValidate, SignValidationProxy>();
        });

        return builder;
    }
}
