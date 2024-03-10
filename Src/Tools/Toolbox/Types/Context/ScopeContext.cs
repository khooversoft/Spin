using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class NullScopeContext
{
    public static ScopeContext Instance { get; } = new ScopeContext(NullLogger.Instance);
}


public readonly record struct ScopeContext : ILoggingContext
{
    [Obsolete("Do not use, logger is required, will throw")]
    public ScopeContext() { throw new InvalidOperationException(); }

    public ScopeContext(ILogger logger, CancellationToken token = default)
    {
        Logger = logger.NotNull();
        Token = token;

        TraceId = Guid.NewGuid().ToString();
    }

    public ScopeContext(string traceId, ILogger logger, CancellationToken token = default)
    {
        TraceId = traceId.NotEmpty();
        Logger = logger.NotNull();
        Token = token;
    }

    public string TraceId { get; }

    [JsonIgnore] public bool IsCancellationRequested => Token.IsCancellationRequested;
    [JsonIgnore] public CancellationToken Token { get; init; }
    [JsonIgnore] public ILogger Logger { get; }
    [JsonIgnore] public ScopeContext Context => this;

    public override string ToString() => "TraceId=" + TraceId;

    public (string? message, object?[] args) AppendContext(string? message, object?[] args)
    {
        return (ScopeContextTools.AppendMessage(message, "traceId={traceId}"), ScopeContextTools.AppendArgs(args, TraceId));
    }

    public ScopeContext With(ILogger logger) => new ScopeContext(TraceId, logger.NotNull(), Token);

    public static implicit operator CancellationToken(ScopeContext context) => context.Token;
}
