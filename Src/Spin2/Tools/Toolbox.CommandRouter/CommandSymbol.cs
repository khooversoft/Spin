using System.Collections;
using System.CommandLine;

namespace Toolbox.CommandRouter;

public class CommandSymbol : IEnumerable<CommandSymbol>
{
    private readonly IList<CommandSymbol> _symbols = new List<CommandSymbol>();

    public CommandSymbol(string name, string? description) => Command = new Command(name, description);

    internal Command Command { get; }

    public CommandSymbol Add(CommandSymbol symbol)
    {
        _symbols.Add(symbol);
        Command.AddCommand(symbol.Command);
        return this;
    }

    public CommandArgument<T> AddArgument<T>(string name, string? description)
    {
        var arg = new CommandArgument<T>(name, description);
        Command.AddArgument(arg.Argument);
        return arg;
    }

    public CommandOption<T> AddOption<T>(string name, string? description, bool isRequired = false)
    {
        var arg = new CommandOption<T>(name, description);
        arg.IsRequired = isRequired;
        Command.AddOption(arg.Option);
        return arg;
    }

    public IEnumerator<CommandSymbol> GetEnumerator() => _symbols.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_symbols).GetEnumerator();
}

