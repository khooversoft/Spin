using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Block.Contract;

public static class ContractStartup
{
    public static IServiceCollection AddBlockHost(this IServiceCollection services)
    {
        services.AddTransient<ContractHost>();

        return services;
    }
}
