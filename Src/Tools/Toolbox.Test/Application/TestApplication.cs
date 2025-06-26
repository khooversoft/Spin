using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

[assembly: InternalsVisibleTo("Toolbox.Azure.test")]

namespace Toolbox.Test.Application;

internal static class TestApplication
{
    public static IServiceProvider CreateServiceProvider(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(outputHelper.WriteLine);
                x.AddDebug();
                x.AddConsole();
                x.AddFilter(x => true);
            })
            .AddInMemoryFileStore()
            .BuildServiceProvider();

        return services;
    }

    public static ILogger<T> CreateLogger<T>(this IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<ILogger<T>>();
    public static ScopeContext CreateScopeContext<T>(this IServiceProvider serviceProvider) => new ScopeContext(serviceProvider.CreateLogger<T>());


}
