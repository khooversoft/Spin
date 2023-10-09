using System.CommandLine;

namespace Toolbox.CommandRouter;

public class CommandArgument<T> : ISymbolDescriptor<T>
{
    public CommandArgument(string name, string? description) => Argument = new Argument<T>(name, description);
    internal Argument<T> Argument { get; }
    public TO GetValueDescriptor<TO>() => (TO)(object)Argument;
}

