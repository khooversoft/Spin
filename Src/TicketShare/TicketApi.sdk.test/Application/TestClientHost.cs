using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
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
            .AddTicketApi(config.GetSection("Ticket").Get<TicketOption>().NotNull())
            .BuildServiceProvider();

        return new TestClientHost(serviceProvider);
    }
}

internal readonly struct TestClientHost
{
    public TestClientHost(IServiceProvider services) => Services = services;
    public IServiceProvider Services { get; init; }
    public TicketEventClient GetEventClient() => Services.GetRequiredService<TicketEventClient>();
    public TicketClassificationClient GetClassificationClient() => Services.GetRequiredService<TicketClassificationClient>();
    public TicketAttractionClient GetAttractionClient() => Services.GetRequiredService<TicketAttractionClient>();
    public ScopeContext GetContext<T>() => Services.GetRequiredService<ILoggerFactory>().CreateLogger<T>().Func(x => new ScopeContext(x));
}
