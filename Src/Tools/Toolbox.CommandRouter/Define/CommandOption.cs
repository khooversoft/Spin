using System.CommandLine;
using Toolbox.Extensions;

namespace Toolbox.CommandRouter;

public class CommandOption<T> : ISymbolDescriptor<T>
{
    public CommandOption(string name, string? description) => Option = new Option<T>(name, description);
    public CommandOption(string name, string? description, T defaulValue) => Option = new Option<T>(name, description).Action(x => x.SetDefaultValue(defaulValue));

    internal Option<T> Option;
    public TO GetValueDescriptor<TO>() => (TO)(object)Option;
    public bool IsRequired { get => Option.IsRequired; set => Option.IsRequired = value; }
}

