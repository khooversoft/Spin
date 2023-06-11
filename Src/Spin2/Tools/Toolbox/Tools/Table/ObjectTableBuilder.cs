using Toolbox.Extensions;

namespace Toolbox.Tools.Table;

public class ObjectTableBuilder
{
    public List<string> Columns { get; } = new List<string>();
    public List<ObjectRow> Rows { get; } = new List<ObjectRow>();

    public ObjectTableBuilder AddColumn(params string[] columns) => this.Action(x => Columns.AddRange(columns));
    public ObjectTableBuilder AddRow(params object?[] columns) => this.Action(x => Rows.Add(new ObjectRow(columns)));
    public ObjectTableBuilder AddRow(params object?[][] rowsColumns) => this.Action(x => rowsColumns.ForEach(x => AddRow(x)));
    public ObjectTableBuilder AddRow(params ObjectRow[] rows) => this.Action(x => rows.ForEach(y => Rows.Add(y)));

    public ObjectTable Build()
    {
        Rows
            .Where(x => x.Cells.Count != Columns.Count)
            .Count()
            .Assert(x => x == 0, x => $"{x} rows data item count does not match column count");

        var header = new TableHeader(Columns);

        return new ObjectTable
        {
            Header = header,
            Rows = Rows.Select(x => new TableRow(x.Cells, header, x.Tag, x.Key)).ToArray(),
        };
    }
}

public record ObjectRow
{
    public ObjectRow(IEnumerable<object?> cells) => Cells = cells.NotNull().ToArray();

    public ObjectRow(IEnumerable<object?> cells, string? tag = null, string? key = null)
    {
        Cells = cells.NotNull().ToArray();
        Tag = tag;
        Key = key;
    }

    public IReadOnlyList<object?> Cells { get; }
    public string? Tag { get; }
    public string? Key { get; }
}
