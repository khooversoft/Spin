using Contract.sdk.Client;
using ContractHost.sdk.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions;

namespace Smart_Installment.sdk;

public static class Extensions
{
    public static IContractHostBuilder AddContractServices(this IContractHostBuilder builder)
    {
        builder.ConfigureService(service =>
        {
            service.AddSingleton<DocumentContractClient>();
        });

        return builder;
    }
}
