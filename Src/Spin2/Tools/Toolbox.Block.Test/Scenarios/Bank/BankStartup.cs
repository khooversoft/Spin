using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.DocumentContainer;
using Toolbox.Tools;
using Toolbox.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Block.Test.Scenarios.Bank;

public static class BankStartup
{
    public static IServiceCollection AddBank(this IServiceCollection services)
    {
        services.AddSingleton<IMessageBroker, MessageBrokerEmulator>();
        services.AddSingleton<ITimeContext, TimeContext>();
        services.AddSingleton<IDocumentStore, DocumentStoreInMemory>();
        services.AddSingleton<DocumentLease>();

        services.AddTransient<BankHost>();

        return services;
    }
}
