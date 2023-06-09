namespace Toolbox.Tools.Table;

public class ObjectTable
{
    public TableHeader Header { get; init; } = null!;
    public IReadOnlyList<TableRow> Rows { get; init; } = Array.Empty<TableRow>();
}
