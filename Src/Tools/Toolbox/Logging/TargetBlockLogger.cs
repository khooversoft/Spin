using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;

namespace Toolbox.Logging;

public class TargetBlockLogger : ILogger
{
    private readonly ITargetBlock<string> _targetBlock;

    public TargetBlockLogger(string name, ITargetBlock<string> targetBlock)
    {
        targetBlock.NotNull();

        Name = name;
        _targetBlock = targetBlock;
    }

    public string Name { get; }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _targetBlock.Post($"{Name}: " + formatter(state, exception));
    }
}