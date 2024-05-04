using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Store;

namespace Toolbox;

public static class ToolboxStartup
{
    public static IServiceCollection AddInMemoryFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton<IFileStore, InMemoryFileStore>();

        return services;
    }
}
