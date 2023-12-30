using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Logging;

public class SimpleConsoleLogging : ILogger
{
    private readonly string _name;
    private readonly object _lock = new object();

    public SimpleConsoleLogging(string name) => _name = name.NotEmpty();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_lock)
        {
            if (!IsEnabled(logLevel)) return;

            string formatString = formatter(state, exception);

            (ConsoleColor? consoleColor, string logText) = logLevel switch
            {
                LogLevel.Information => (ConsoleColor.Green, "Info"),
                LogLevel.Warning => (ConsoleColor.Yellow, "Warn"),
                LogLevel.Error => (ConsoleColor.Red, "Error"),
                LogLevel.Critical => (ConsoleColor.Red, "Critial"),

                _ => ((ConsoleColor?)null, logLevel.ToString()),
            };

            ConsoleColor originalColor = Console.ForegroundColor;

            Console.Write($"{DateTime.Now.ToString("HH:mm:ss")} ");

            Console.ForegroundColor = consoleColor ?? originalColor;
            Console.Write($"{logText,-6} ");
            Console.ForegroundColor = originalColor;

            if (logLevel <= LogLevel.Information)
            {
                int index = formatString.IndexOf("traceId");
                if (index >= 0)
                {
                    while (index > 0 && formatString[index - 1] == ' ' || formatString[index - 1] == ',') index--;
                    formatString = formatString[0..index];
                }
            }

            Console.WriteLine(formatString);
        }
    }
}


[UnsupportedOSPlatform("browser")]
[ProviderAlias("ColorConsole")]
public sealed class ColorConsoleLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, SimpleConsoleLogging> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new SimpleConsoleLogging(name));

    public void Dispose() => _loggers.Clear();
}

public static class ColorConsoleLoggerProviderExtension
{
    public static ILoggingBuilder SimpleConsole(this ILoggingBuilder builder)
    {
        builder.AddProvider(new ColorConsoleLoggerProvider());
        return builder;
    }
}

public record Circle { public double Radius { get; } }
public record Rectangle { public int Width { get; } public int Height { get; } }

public static class AA
{
    public static int CalculateArea(object shape)
    {
        switch (shape)
        {
            case Circle circle:
                return (int)(Math.PI * circle.Radius * circle.Radius);
            case Rectangle rectangle:
                return rectangle.Width * rectangle.Height;
            default:
                throw new ArgumentException("Unknown shape");
        }
    }

    public static int CalculateArea1(object shape) => shape switch
    {
        Circle circle => (int)(Math.PI * circle.Radius * circle.Radius),
        Rectangle rectangle => rectangle.Width * rectangle.Height,
        _ => throw new ArgumentException("Unknown shape"),
    };
}