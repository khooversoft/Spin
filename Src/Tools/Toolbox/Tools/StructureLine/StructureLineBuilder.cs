using System.Diagnostics;
using System.Text;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public sealed class StructureLineBuilder
{
    private readonly List<object?> _allArgs = new();
    private readonly StringBuilder _messageBuilder = new();

    public static StructureLineBuilder Start() => new();

    public StructureLineBuilder Add(string? message, object? value)
    {
        if (message.IsEmpty()) return this;

        if (_messageBuilder.Length > 0) _messageBuilder.Append(", ");
        _messageBuilder.Append(message);

        if (value != null) _allArgs.Add(value);

        return this;
    }

    public StructureLineBuilder Add(string? message, params IEnumerable<object?>? args)
    {
        if (message.IsEmpty()) return this;

        if (_messageBuilder.Length > 0) _messageBuilder.Append(", ");

        _messageBuilder.Append(message);

        if (args != null)
        {
            foreach (var arg in args) _allArgs.Add(arg);
        }

        return this;
    }

    public StructureLineRecord Build()
    {
        var record = new StructureLineRecord(_messageBuilder.ToString(), _allArgs);

#if DEBUG
        var variableCount = record.GetVariables().Count;
        if (variableCount != record.Args.Length) Debugger.Break();

        variableCount.Be(record.Args.Length, "Unbalanced parameter.Count != args.Count");
#endif

        return record;
    }
}
