namespace Toolbox.Types;

public readonly record struct ScopeContextLocation
{
    public ScopeContextLocation(ScopeContext context, CodeLocation location, ILoggingFormatter formatter)
    {
        Context = context;
        Location = location;
        Formatter = formatter;
    }

    public ScopeContext Context { get; init; }
    public CodeLocation Location { get; }
    public ILoggingFormatter Formatter { get; }
}