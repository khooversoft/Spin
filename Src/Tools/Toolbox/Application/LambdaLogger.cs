using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Application;

public class LambdaLoggerProvider : ILoggerProvider
{
    private readonly Action<string> _redirect;
    public LambdaLoggerProvider(Action<string> redirect) => _redirect = redirect;
    public ILogger CreateLogger(string categoryName) => new LambdaLogger(_redirect, categoryName);
    public void Dispose() { }
}


public static class LambdaLoggerProviderExtension
{
    public static ILoggingBuilder AddLambda(this ILoggingBuilder builder, Action<string> lambda)
    {
        builder.AddProvider(new LambdaLoggerProvider(lambda));
        return builder;
    }
}



public class LambdaLogger : ILogger
{
    private readonly Action<string> _redirect;
    private readonly string _categoryName;

    public LambdaLogger(Action<string> redirect, string categoryName)
    {
        _redirect = redirect.NotNull();
        _categoryName = categoryName.NotEmpty();
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopDisposable.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _redirect($"{_categoryName} [{eventId}] {formatter(state, exception)}");
        if (exception != null) _redirect(exception.ToString());
    }

    private class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new NoopDisposable();
        public void Dispose() { }
    }
}