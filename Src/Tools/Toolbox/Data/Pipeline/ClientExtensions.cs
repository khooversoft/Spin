using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class ClientExtensions
{
    public static IDataClient GetDataClient(this IServiceProvider serviceProvider, string name = "default")
    {
        serviceProvider.NotNull();
        name.NotEmpty();
        var factory = serviceProvider.GetRequiredService<DataClientFactory>();
        return factory.Create(name);
    }

    public static IDataClient<T> GetDataClient<T>(this IServiceProvider serviceProvider)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<DataClientFactory>();
        return factory.Create<T>();
    }

    public static IJournalClient<T> GetJournalClient<T>(this IServiceProvider serviceProvider)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<JournalClientFactory>();
        return factory.Create<T>();
    }
}
