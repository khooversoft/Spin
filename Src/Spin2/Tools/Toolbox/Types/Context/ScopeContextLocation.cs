using Microsoft.Extensions.Logging;

namespace Toolbox.Types;

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