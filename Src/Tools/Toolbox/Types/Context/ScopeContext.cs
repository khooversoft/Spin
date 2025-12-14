using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Types;

// TODO: remove "CancelationToken"
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

    public ScopeContext With(ILogger logger) => new(TraceId, logger, CancellationToken);
    public ScopeContext With(CancellationToken token) => new(TraceId, Logger, token);
    public ScopeContext WithNewTraceId() => new(Logger, CancellationToken);

    public static implicit operator CancellationToken(ScopeContext context) => context.CancellationToken;
}


public static class ScopeContextTool
{
    public static ScopeContext CreateContext<T>(this IServiceProvider service) => service.NotNull()
        .GetRequiredService<ILogger<T>>()
        .ToScopeContext();
}
