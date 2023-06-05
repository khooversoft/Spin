using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct ScopeContext
{
    public ScopeContext(CancellationToken? token = null)
    {
        TraceId = Guid.NewGuid().ToString();
        Token = token;
    }

    public ScopeContext(string traceId, CancellationToken? token = null)
    {
        traceId.NotEmpty();
        TraceId = traceId;
        Token = token;
    }

    public static ScopeContext Default { get; } = new ScopeContext();

    public string TraceId { get; }

    [JsonIgnore]
    public CancellationToken? Token { get; init; }
    public bool IsCancellationRequested => Token?.IsCancellationRequested ?? false;

    [JsonIgnore]
    public ILogger? Logger { get; init; }

    public ScopeContextLocation Location([CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0)
    {
        return new ScopeContextLocation(this, new CodeLocation(function, path, lineNumber));
    }

    public static implicit operator CancellationToken(ScopeContext context) => context.Token ?? default;
}


public readonly record struct ScopeContextLocation
{
    public ScopeContextLocation(ScopeContext context, CodeLocation location)
    {
        Context = context;
        Location = location;
    }

    public ScopeContext Context { get; init; }
    public CodeLocation Location { get; }

    public ScopeContextLocation With(ILogger logger) => this with
    {
        Context = this.Context with { Logger = logger },
    };
}