using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk.test.Application;

internal static class TestClientHostTool
{
    public static TestClientHost Create()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("Application/test-appsettings.json")
            .AddUserSecrets("TicketMasterApi.sdk.test-c1184340-447c-4c3d-8a49-09d46b80b30b")
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddLogging(config => config.AddDebug())
            .AddSingleton(config)
            .AddSingleton<IMemoryCache, NullMemoryCache>()
            .AddTicketApi(config.Get<TicketOption>("Ticket", TicketOption.Validator))
            .AddDatalakeFileStore(config.Get<DatalakeOption>("Storage", DatalakeOption.Validator))
            .BuildServiceProvider();

        return new TestClientHost(serviceProvider);
    }
}

internal class TestClientHost : IDisposable
{
    private ServiceProvider? _services;
    public TestClientHost(ServiceProvider services) => _services = services.NotNull();
    public IServiceProvider Services => _services.NotNull();
    public ScopeContext GetContext<T>() => Services.GetRequiredService<ILoggerFactory>().CreateLogger<T>().Func(x => new ScopeContext(x));

    public void Dispose() => Interlocked.Exchange(ref _services, null)?.Dispose();
}
