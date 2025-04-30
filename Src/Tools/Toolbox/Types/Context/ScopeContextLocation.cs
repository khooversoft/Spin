using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct ScopeContextLocation : ILoggingContext
{
    private const string _addMessage = "traceId={traceId}, callerFunction={callerFunction}, callerFilePath={callerFilePath}, callerLineNumber={callerLineNumber}";

    public ScopeContextLocation(ScopeContext context, CodeLocation location)
    {
        Context = context;
        Location = location;
    }

    public ScopeContext Context { get; init; }
    public CodeLocation Location { get; }

    public (string? message, object?[] args) AppendContext(string? message, object?[] args)
    {
        return (
            ScopeContextTools.AppendMessage(message, _addMessage),
            ScopeContextTools.AppendArgs(args, Context.TraceId, Location.CallerFunction, Location.CallerFilePath, Location.CallerLineNumber)
            );
    }
}


public static class ScopeContextLocationExtensions
{
    public static ScopeContextLocation Location(
        this ScopeContext context,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0
        )
    {
        return new ScopeContextLocation(context, new CodeLocation(function, path, lineNumber));
    }
}