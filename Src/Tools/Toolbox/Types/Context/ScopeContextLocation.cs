namespace Toolbox.Types;

public readonly record struct ScopeContextLocation : ILoggingContext
{
    public ScopeContextLocation(ScopeContext context, CodeLocation location)
    {
        Context = context;
        Location = location;
    }

    public ScopeContext Context { get; init; }
    public CodeLocation Location { get; }

    public (string? message, object?[] args) AppendContext(string? message, object?[] args)
    {
        string addMessage = "traceId={traceId}, callerFunction={callerFunction}, callerFilePath={callerFilePath}, callerLineNumber={callerLineNumber}";

        return (
            ScopeContextTools.AppendMessage(message, addMessage),
            ScopeContextTools.AppendArgs(args, Context.TraceId, Location.CallerFunction, Location.CallerFilePath, Location.CallerLineNumber)
            );
    }
}
