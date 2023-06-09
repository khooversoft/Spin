using Toolbox.Extensions;

namespace Toolbox.Tools.Table;

public class ObjectTableBuilder
{
    public List<string> Columns { get; } = new List<string>();
    public List<object[]> Rows { get; } = new List<object[]>();

    public ObjectTableBuilder AddCoumn(params string[] columns) => this.Action(x => Columns.AddRange(columns));
    public ObjectTableBuilder AddRow(params object[] columns) => this.Action(x => Rows.Add(columns));
    public ObjectTableBuilder AddRow(params object[][] rowsColumns) => this.Action(x => rowsColumns.ForEach(x => Rows.Add(x)));
    public ObjectTableBuilder AddRow(params IEnumerable<object>[] rowsColumns) => this.Action(x => rowsColumns.ForEach(x => Rows.Add(x.ToArray())));

    public ObjectTable Build()
    {
        Rows
            .Where(x => x.Length != Columns.Count)
            .Count()
            .Assert(x => x == 0, x => $"{x} rows data item count does not match column count");

        var header = new TableHeader(Columns);

        return new ObjectTable
        {
            Header = header,
            Rows = Rows.Select(x => new TableRow(x, header)).ToArray(),
        };
    }
}
