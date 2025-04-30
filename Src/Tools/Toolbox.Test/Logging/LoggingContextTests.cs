using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Logging;

public class LoggingContextTests
{
    [Fact]
    public void SingleLogLogCritical()
    {
        ConcurrentQueue<string> queue = new();
        ScopeContext context = CreateService<LoggingContextTests>(x => queue.Enqueue(x));

        context.LogCritical("Test");
        queue.Count.Be(1);
        queue.ToSeq().Next().StartsWith("[Critical] Toolbox.Test.Logging.LoggingContextTests [0] Test, traceId=").BeTrue();
    }

    [Fact]
    public void SingleLogLogError()
    {
        ConcurrentQueue<string> queue = new();
        ScopeContext context = CreateService<LoggingContextTests>(x => queue.Enqueue(x));

        context.LogError("Test");
        queue.Count.Be(1);
        queue.ToSeq().Next().StartsWith("[Error] Toolbox.Test.Logging.LoggingContextTests [0] Test, traceId=").BeTrue();
    }

    [Fact]
    public void SingleLogLogWarning()
    {
        ConcurrentQueue<string> queue = new();
        ScopeContext context = CreateService<LoggingContextTests>(x => queue.Enqueue(x));

        context.LogWarning("Test");
        queue.Count.Be(1);
        queue.ToSeq().Next().StartsWith("[Warning] Toolbox.Test.Logging.LoggingContextTests [0] Test, traceId=").BeTrue();
    }

    [Fact]
    public void SingleLogInformation()
    {
        ConcurrentQueue<string> queue = new();
        ScopeContext context = CreateService<LoggingContextTests>(x => queue.Enqueue(x));

        context.LogInformation("Test");
        queue.Count.Be(1);
        queue.ToSeq().Next().StartsWith("[Information] Toolbox.Test.Logging.LoggingContextTests [0] Test, traceId=").BeTrue();
    }

    [Fact]
    public void SingleLogDebug()
    {
        ConcurrentQueue<string> queue = new();
        ScopeContext context = CreateService<LoggingContextTests>(x => queue.Enqueue(x));

        context.LogDebug("Test");
        queue.Count.Be(1);
        queue.ToSeq().Next().StartsWith("[Debug] Toolbox.Test.Logging.LoggingContextTests [0] Test, traceId=").BeTrue();
    }

    [Fact]
    public void SingleLogTrace()
    {
        ConcurrentQueue<string> queue = new();
        ScopeContext context = CreateService<LoggingContextTests>(x => queue.Enqueue(x));

        context.LogTrace("Test");
        queue.Count.Be(1);
        queue.ToSeq().Next().StartsWith("[Trace] Toolbox.Test.Logging.LoggingContextTests [0] Test, traceId=").BeTrue();
    }

    [Fact]
    public void SingleLogInformationSingleParameter()
    {
        ConcurrentQueue<string> queue = new();
        ScopeContext context = CreateService<LoggingContextTests>(x => queue.Enqueue(x));

        context.LogInformation("Test, value={value}", 10);
        queue.Count.Be(1);
        queue.ToSeq().Next().StartsWith("[Information] Toolbox.Test.Logging.LoggingContextTests [0] Test, value=10, ").BeTrue();
    }

    [Fact]
    public void SingleLogInformationSingleParameterAndLocation()
    {
        ConcurrentQueue<string> queue = new();
        var context = CreateService<LoggingContextTests>(x => queue.Enqueue(x)).Location();

        context.LogInformation("Test, value={value}", 10);
        queue.Count.Be(1);
        queue.ToSeq().Next().Action(x =>
        {
            x.StartsWith("[Information] Toolbox.Test.Logging.LoggingContextTests [0] Test, value=10, ").BeTrue();
            x.IndexOf("callerFunction=SingleLogInformationSingleParameterAndLocation").Assert(x => x > 0, "Function not found");
            x.IndexOf("callerFilePath=").Assert(x => x > 0, "File not found");
            x.IndexOf("callerLineNumber=").Assert(x => x > 0, "Line not found");
        });
    }

    private ScopeContext CreateService<T>(Action<string> saveAction)
    {
        saveAction.NotNull();

        var services = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddLambda(saveAction);
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .BuildServiceProvider();

        ILogger<T> logger = services.GetRequiredService<ILogger<T>>();
        return new ScopeContext(logger);
    }
}
