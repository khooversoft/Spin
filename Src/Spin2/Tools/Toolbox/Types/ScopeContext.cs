using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct ScopeContext
{
    public ScopeContext(CancellationToken? token = null)
    {
        TraceId = Guid.NewGuid().ToString();
        Token = token;
    }

    public ScopeContext(string workId, CancellationToken? token = null)
    {
        workId.NotEmpty();
        TraceId = workId;
        Token = token;
    }

    public string TraceId { get; }

    [JsonIgnore]
    public CancellationToken? Token { get; init; }

    public ScopeContextLocation Location([CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0)
    {
        return new ScopeContextLocation(this, new CodeLocation(function, path, lineNumber));
    }
}


public readonly record struct ScopeContextLocation
{
    public ScopeContextLocation(ScopeContext context, CodeLocation location)
    {
        Context = context;
        Location = location;
    }

    public ScopeContext Context { get; }
    public CodeLocation Location { get; }
}