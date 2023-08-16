using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct ScopeContext
{
    private readonly ILogger? _logger;

    [JsonConstructor]
    public ScopeContext(string traceId) => TraceId = traceId.NotEmpty();

    public ScopeContext(ILogger logger, CancellationToken token = default)
    {
        _logger = logger.NotNull();
        Token = token;

        TraceId = Guid.NewGuid().ToString();
    }

    public ScopeContext(string traceId, ILogger logger, CancellationToken token = default)
    {
        traceId.NotEmpty();
        _logger = logger.NotNull();
        TraceId = traceId;
        Token = token;
    }

    public string TraceId { get; }

    [JsonIgnore] public bool IsCancellationRequested => Token.IsCancellationRequested;
    [JsonIgnore] public CancellationToken Token { get; init; }
    [JsonIgnore] public ILogger Logger => _logger.NotNull();

    public ScopeContextLocation Location([CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0)
    {
        Logger.NotNull();
        return new ScopeContextLocation(this, new CodeLocation(function, path, lineNumber));
    }

    public ScopeContext With(ILogger logger) => new ScopeContext(TraceId, logger.NotNull(), Token);

    public static implicit operator CancellationToken(ScopeContext context) => context.Token;
}
