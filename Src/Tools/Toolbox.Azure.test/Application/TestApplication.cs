using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Application;

internal static class TestApplication
{
    public static IKeyStore GetDatalake(string basePath) => new DatalakeStore(ReadOption(basePath), new NullLogger<DatalakeStore>());

    public static DatalakeOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Get<DatalakeOption>().NotNull()
        .Func(x => x with { BasePath = basePath });

    public static ScopeContext CreateScopeContext<T>(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(outputHelper.WriteLine);
                x.AddDebug();
                x.AddConsole();
                x.AddFilter(x => true);
            })
            .BuildServiceProvider();

        ILogger<T> logger = services.GetRequiredService<ILogger<T>>();
        return new ScopeContext(logger);
    }
}
