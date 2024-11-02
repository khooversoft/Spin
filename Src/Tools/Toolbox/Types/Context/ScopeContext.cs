using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class NullScopeContext
{
    public static ScopeContext Default { get; } = new ScopeContext(NullLogger.Instance);
}


public readonly record struct ScopeContext : ILoggingContext
{
    [Obsolete("Do not use, logger is required, will throw")]
    public ScopeContext() { throw new InvalidOperationException(); }

    public ScopeContext(ILogger logger, CancellationToken token = default)
    {
        Logger = logger.NotNull();
        CancellationToken = token;

        TraceId = Guid.NewGuid().ToString();
    }

    public ScopeContext(string traceId, ILogger logger, CancellationToken token = default)
    {
        TraceId = traceId.NotEmpty();
        Logger = logger.NotNull();
        CancellationToken = token;
    }

    public string TraceId { get; }

    [JsonIgnore] public bool IsCancellationRequested => CancellationToken.IsCancellationRequested;
    [JsonIgnore] public CancellationToken CancellationToken { get; init; }
    [JsonIgnore] public ILogger Logger { get; }
    [JsonIgnore] public ScopeContext Context => this;

    public override string ToString() => "TraceId=" + TraceId;

    public (string message, object?[] args) AppendContext(string? message, object?[] args)
    {
        return (ScopeContextTools.AppendMessage(message, "traceId={traceId}"), ScopeContextTools.AppendArgs(args, TraceId));
    }

    public ScopeContext With(ILogger logger) => new ScopeContext(TraceId, logger.NotNull(), CancellationToken);

    public static implicit operator CancellationToken(ScopeContext context) => context.CancellationToken;
}
