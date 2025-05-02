using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class StructureLineBuilder
{
    public static IEnumerable<StructureLineRecord> Start() => Array.Empty<StructureLineRecord>();

    public static IEnumerable<StructureLineRecord> Add(this IEnumerable<StructureLineRecord> subject, string? message, object? value)
    {
        if (message.IsEmpty()) return subject;

        var record = new StructureLineRecord(message, value);
        return subject.Append(record);
    }

    public static IEnumerable<StructureLineRecord> Add(this IEnumerable<StructureLineRecord> subject, string? message, params IEnumerable<object?>? args)
    {
        if (message.IsEmpty()) return subject;

        var record = new StructureLineRecord(message, args);
        return subject.Append(record);
    }

    public static StructureLineRecord Build(this IEnumerable<StructureLineRecord> subject)
    {
        subject.NotNull();

        string message = subject.Select(x => x.Message).OfType<string>().Join(", ");
        var args = subject.SelectMany(x => x.Args);

        var record = new StructureLineRecord(message, args);

        var variableCount = record.GetVariables().Count;
        variableCount.Be(record.Args.Length, "Unbalanced parameter.Count != args.Count");

        return record;
    }
}