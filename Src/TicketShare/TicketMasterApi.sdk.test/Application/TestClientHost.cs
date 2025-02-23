using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketMasterApi.sdk.test.Application;

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
            .AddTicketMaster(config.GetSection("TicketMaster"))
            .BuildServiceProvider();

        return new TestClientHost(serviceProvider);
    }
}

internal readonly struct TestClientHost
{
    public TestClientHost(IServiceProvider services) => Services = services;
    public IServiceProvider Services { get; init; }
    public TicketMasterDiscoverClient GetClient() => Services.GetRequiredService<TicketMasterDiscoverClient>();
    public ScopeContext GetContext<T>() => Services.GetRequiredService<ILoggerFactory>().CreateLogger<T>().Func(x => new ScopeContext(x));
}
