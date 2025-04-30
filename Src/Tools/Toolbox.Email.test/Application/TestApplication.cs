using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Email;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Application;

internal static class TestApplication
{
    public static TestApplicationContext Create<T>(ITestOutputHelper outputHelper)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("TestSettings.json")
            .AddUserSecrets("Toolbox-Email-Dev")
            .Build();

        ServiceProvider services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(outputHelper.WriteLine);
                x.AddDebug();
                x.AddConsole();
            })
            .AddEmail(config.GetSection("email").Get<EmailOption>().NotNull())
            .BuildServiceProvider();

        return new TestApplicationContext(services);
    }
}

public readonly struct TestApplicationContext
{
    internal TestApplicationContext(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;
    public IServiceProvider ServiceProvider { get; }
    public ILogger<T> CreateLogger<T>() => ServiceProvider.GetRequiredService<ILogger<T>>();
    public ScopeContext CreateContext<T>() => new ScopeContext(CreateLogger<T>());
}
