using Bank.sdk.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Spin.Common.Model;
using Spin.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Extensions;
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
