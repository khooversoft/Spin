using System.Diagnostics;
using System.Text;
using Toolbox.Extensions;

namespace Toolbox.Tools.Should;

public readonly struct ShouldContext<T>
{
    public ShouldContext(T value, string function, string path, int lineNumber, string argName)
    {
        Value = value;
        Function = function;
        Path = path;
        LineNumber = lineNumber;
        ArgName = argName;
    }

    public T Value { get; }

    public string Function { get; }
    public string Path { get; }
    public int LineNumber { get; }
    public string ArgName { get; }

    public string BuildMessage(string message, string? because)
    {
        message.NotEmpty();

        var str = new StringBuilder(message);
        if (because.IsNotEmpty()) str.Append($", because: {because}");
        str.Append($", Function={Function}, Path={Path}, LineNumber={LineNumber}, ArgName={ArgName}");

        return str.ToString();
    }
}


[DebuggerStepThrough]
public static class ShouldContextExtensions
{
    public static void ThrowException<T>(this ShouldContext<T> subject, string message, string? because = null, Exception? ex = null)
    {
        throw new ArgumentException(subject.BuildMessage(message, because), ex);
    }
}