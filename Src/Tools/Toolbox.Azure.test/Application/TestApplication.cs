using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Application;

internal static class TestApplication
{
    public static IDatalakeStore GetDatalake(string basePath) => new DatalakeStore(ReadOption(basePath), new NullLogger<DatalakeStore>());

    private static DatalakeOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Bind<DatalakeOption>()
        .Func(x => x with { BasePath = basePath });

    public static ScopeContext CreateScopeContext<T>(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(outputHelper.WriteLine);
                x.AddDebug();
                x.AddConsole();
            })
            .BuildServiceProvider();

        ILogger<T> logger = services.GetRequiredService<ILogger<T>>();
        return new ScopeContext(logger);
    }
}
