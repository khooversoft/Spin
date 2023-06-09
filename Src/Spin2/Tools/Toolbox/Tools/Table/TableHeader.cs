namespace Toolbox.Tools.Table;

public class TableHeader
{
    public TableHeader(IEnumerable<string> columns)
    {
        columns.NotNull();
        Columns = columns.Select((x, i) => new TableColumn { Index = i, Name = x }).ToArray();
        ByName = Columns.Select((x, i) => (x, i)).ToDictionary(x => x.x.Name, x => x.i);
    }

    public IReadOnlyList<TableColumn> Columns { get; }
    public IReadOnlyDictionary<string, int> ByName { get; }
}

public record TableColumn
{
    public int Index { get; init; }
    public string Name { get; init; } = null!;
}