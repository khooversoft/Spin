using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public enum TraceType
{
    None,
    Start,
    Process,
    Ok,
    Error
}


public record LangTrace
{
    public string Action { get; init; } = null!;
    public TraceType Type { get; init; }
    public string Name { get; init; } = null!;
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; } = null!;
    public string TokenPointer { get; init; } = null!;
    public string SyntaxPointer { get; init; } = null!;

    public override string ToString() => Error switch
    {
        null => $"StatusCode={StatusCode,-10}, Type={Type,-7}, Action={Action,-8}, Name={Name, -12}, TokenPointer={TokenPointer}, SyntaxPointer={SyntaxPointer}",
        not null => $"StatusCode={StatusCode,-10}, Type={Type,-7}, Action={Action,-8}, Name={Name, -12}, Error={Error}, TokenPointer={TokenPointer}, SyntaxPointer={SyntaxPointer}",
    };
}


[DebuggerStepThrough]
public static class LangTraceExtensions
{
    public static string TokenPointer(this LangParserContext subject)
    {
        return "[ " + subject.TokensCursor.List.Skip(subject.TokensCursor.Index + 1).Select(x => x.ToString()).Join(' ') + " ]";
    }

    public static void Log(this LangParserContext subject, TraceType type, string action, string? name = null)
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

    public static void Log(this LangParserContext subject, TraceType type, string action, Option option, string? name = null)
    {
        var trace = new LangTrace
        {
            Action = action,
            Type = type,
            Name = name ?? "<no name>",
            StatusCode = option.StatusCode,
            Error = option.Error,
            TokenPointer = subject.TokenPointer(),
            SyntaxPointer = subject.Root.ToString() ?? "<no root>",
        };

        subject.Trace.Add(trace);
    }

    public static void Log<T>(this LangParserContext subject, TraceType type, string action, Option<T> option, string? name = null)
    {
        var trace = new LangTrace
        {
            Action = action,
            Type = type,
            Name = name ?? "<no name>",
            StatusCode = option.StatusCode,
            Error = option.Error,
            TokenPointer = subject.TokenPointer(),
            SyntaxPointer = subject.Root.ToString() ?? "<no root>",
        };

        subject.Trace.Add(trace);
    }
}