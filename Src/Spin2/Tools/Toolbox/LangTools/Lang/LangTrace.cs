using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public enum LangTraceType
{
    None,
    Start,
    Result,
    Detail,
}

public record LangTrace
{
    public string Action { get; init; } = null!;
    public LangTraceType Type { get; init; }
    public string Name { get; init; } = null!;
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; } = null!;
    public string TokenPointer { get; init; } = null!;
    public string SyntaxPointer { get; init; } = null!;

    public override string ToString() => Error switch
    {
        null => $"StatusCode={StatusCode}, Type={Type}, Action={Action}, Name={Name}, TokenPointer={TokenPointer}, SyntaxPointer={SyntaxPointer}",
        not null => $"StatusCode={StatusCode}, Type={Type}, Action ={Action}, Name={Name}, Error={Error}, TokenPointer={TokenPointer}, SyntaxPointer={SyntaxPointer}",
    };
}


public static class LangTraceExtensions
{
    public static string TokenPointer(this LangParserContext subject)
    {
        return "[ " + subject.TokensCursor.List.Skip(subject.TokensCursor.Index).Select(x => x.ToString()).Join(' ') + " ]";
    }

    public static Option RunAndLog(this LangParserContext subject, string action, string? name, Func<Option> exec)
    {
        subject.NotNull();
        action.NotEmpty();
        exec.NotNull();

        subject.Log(LangTraceType.Start, action, name);
        var result = exec();
        subject.Log(action, result, name);

        return result;
    }

    public static Option<T> RunAndLog<T>(this LangParserContext subject, string action, string? name, Func<Option<T>> exec)
    {
        subject.NotNull();
        action.NotEmpty();
        exec.NotNull();

        subject.Log(LangTraceType.Start, action,  name);
        var result = exec();
        subject.Log(action, result, name);

        return result;
    }

    public static void Log(this LangParserContext subject, LangTraceType type, string action, string? name = null)
    {
        var trace = new LangTrace
        {
            Action = action,
            Type = type,
            Name = name ?? "<no name>",
            TokenPointer = subject.TokenPointer(),
            SyntaxPointer = subject.Root.ToString() ?? "<no root>",
        };

        subject.Trace.Add(trace);
    }

    public static void Log(this LangParserContext subject, string action, Option option, string? name = null)
    {
        var trace = new LangTrace
        {
            Action = action,
            Type = LangTraceType.Result,
            Name = name ?? "<no name>",
            StatusCode = option.StatusCode,
            Error = option.Error,
            TokenPointer = subject.TokenPointer(),
            SyntaxPointer = subject.Root.ToString() ?? "<no root>",
        };

        subject.Trace.Add(trace);
    }

    public static void Log<T>(this LangParserContext subject, string action, Option<T> option, string? name = null)
    {
        var trace = new LangTrace
        {
            Action = action,
            Type = LangTraceType.Result,
            Name = name ?? "<no name>",
            StatusCode = option.StatusCode,
            Error = option.Error,
            TokenPointer = subject.TokenPointer(),
            SyntaxPointer = subject.Root.ToString() ?? "<no root>",
        };

        subject.Trace.Add(trace);
    }
}