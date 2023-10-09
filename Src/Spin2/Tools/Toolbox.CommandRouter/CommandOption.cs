using System.CommandLine;

namespace Toolbox.CommandRouter;

public class CommandOption<T> : ISymbolDescriptor<T>
{
    public CommandOption(string name, string? description) => Option = new Option<T>(name, description);

    internal Option<T> Option;
    public TO GetValueDescriptor<TO>() => (TO)(object)Option;
}

