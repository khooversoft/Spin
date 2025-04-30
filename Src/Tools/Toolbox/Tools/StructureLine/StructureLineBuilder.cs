using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Tools;

public class StructureLineBuilder
{
    private Sequence<StructureLineRecord> _args = new();

    public StructureLineBuilder Add(string? message)
    {
        if (message.IsNotEmpty()) _args += new StructureLineRecord(message);
        return this;
    }

    public StructureLineBuilder Add(string? message, params IEnumerable<object?>? args)
    {
        if (message.IsNotEmpty()) _args += new StructureLineRecord(message, args);
        return this;
    }

    public StructureLineBuilder Add(StructureLineRecord record) => this.Action(_ => _args += record.NotNull());

    public StructureLineRecord Build()
    {
        _args.Count.Assert(x => x > 0, _ => "No records to build");

        string message = _args.Select(x => x.Message).OfType<string>().Join(", ");
        var args = _args.SelectMany(x => x.Args).ToArray();

        var record = new StructureLineRecord(message, args);

        var variableCount = record.GetVariables().Count;
        variableCount.Be(args.Length, "Unbalanced parameter.Count != args.Count");

        return record;
    }
}
