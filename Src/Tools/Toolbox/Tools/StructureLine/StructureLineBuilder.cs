using System.Diagnostics;
using System.Text;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public sealed class StructureLineBuilder
{
    private readonly List<object?> _allArgs = new();
    private readonly StringBuilder _messageBuilder = new();

    public static StructureLineBuilder Start() => new();

    // Convenience: message with no args, avoids params array allocation.
    public StructureLineBuilder Add(string? message)
    {
        if (message.IsEmpty()) return this;

        if (_messageBuilder.Length > 0) _messageBuilder.Append(", ");
        _messageBuilder.Append(message);
        return this;
    }

    public StructureLineBuilder Add(string? message, object? value)
    {
        if (message.IsEmpty()) return this;

        if (_messageBuilder.Length > 0) _messageBuilder.Append(", ");
        _messageBuilder.Append(message);

        // Preserve existing behavior: skip null so mismatches are detected in Build().
        if (value != null) _allArgs.Add(value);

        return this;
    }

    // Fix: accept normal params array and flatten into the args list.
    public StructureLineBuilder Add(string? message, params object?[] args)
    {
        if (message.IsEmpty()) return this;

        if (_messageBuilder.Length > 0) _messageBuilder.Append(", ");
        _messageBuilder.Append(message);

        if (args is { Length: > 0 }) _allArgs.AddRange(args);

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
