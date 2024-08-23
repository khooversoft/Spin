using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Application;

public abstract class TestBase
{
    private readonly ITestOutputHelper _output;


    public TestBase(ITestOutputHelper output)
    {
        _output = output.NotNull();

        Services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(_output.WriteLine);
                x.AddDebug();
                x.AddConsole();
            })
            .BuildServiceProvider();
    }

    public ServiceProvider Services { get; }

    public ScopeContext GetScopeContext<T>() where T : notnull
    {
        ILogger<T> logger = Services.GetRequiredService<ILogger<T>>();
        return new ScopeContext(logger);
    }
}

public abstract class TestBase<T> : TestBase where T : notnull
{
    public TestBase(ITestOutputHelper output) : base(output) { }

    public ScopeContext GetScopeContext()
    {
        ILogger<T> logger = Services.GetRequiredService<ILogger<T>>();
        return new ScopeContext(logger);
    }
}