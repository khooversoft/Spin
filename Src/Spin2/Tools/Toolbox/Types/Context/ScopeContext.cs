using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct ScopeContext
{
    private readonly ILogger _logger = NullLogger.Instance;

    public ScopeContext(CancellationToken token = default)
    {
        TraceId = Guid.NewGuid().ToString();
        Token = token;
    }

    public ScopeContext(ILogger logger, CancellationToken token = default)
    {
        _logger = logger.NotNull();
        TraceId = Guid.NewGuid().ToString();
        Token = token;
    }

    public ScopeContext(string traceId, CancellationToken token = default)
    {
        traceId.NotEmpty();
        TraceId = traceId;
        Token = token;
    }

    public ScopeContext(string traceId, ILogger logger, CancellationToken token = default)
    {
        traceId.NotEmpty();
        TraceId = traceId;
        Token = token;
    }

    public string TraceId { get; }
    public static ScopeContext Default { get; } = new ScopeContext();
    public bool IsCancellationRequested => Token.IsCancellationRequested;

    [JsonIgnore] public CancellationToken Token { get; init; }
    [JsonIgnore] public ILogger Logger { get => _logger.NotNull(); }

    public ScopeContextLocation Location([CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0)
    {
        return new ScopeContextLocation(this, new CodeLocation(function, path, lineNumber));
    }

    public ScopeContext With(ILogger logger) => new ScopeContext(TraceId, logger, Token);

    public static implicit operator CancellationToken(ScopeContext context) => context.Token;
}
