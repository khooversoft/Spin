using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class ObjectTableTests
{
    [Fact]
    public void ObjectTableEmptyTest()
    {
        ObjectTable table = new ObjectTableBuilder()
            .Build();

        table.Header.Columns.Count.Be(0);
        table.Rows.Count.Be(0);
    }

    [Fact]
    public void ObjectTableSimpleTest()
    {
        const string column1Text = "first";
        const string data1Test = "line #1";

        ObjectTable table = new ObjectTableBuilder()
            .AddColumn(column1Text)
            .AddRow(data1Test)
            .Build();

        table.Header.Columns.Count.Be(1);
        table.Header.Columns[0].Name.Be(column1Text);

        table.Rows.Count.Be(1);
        table.Rows[0].Header.NotNull();
        table.Rows[0].Items.Count.Be(1);
        table.Rows[0].Items[0].Value.Assert(x => x?.Cast<string>() == data1Test);
        table.Rows[0].Items[0].Get<string>().Be(data1Test);
        table.Rows[0].Get<string>(0).Be(data1Test);
        table.Rows[0].Get<string>(column1Text).Be(data1Test);
    }

    [Fact]
    public void ObjectTableSimpleTwoRowsTest()
    {
        const string column1Text = "first";
        const string data1Test = "line #1";
        const string data2Test = "line #2";

        ObjectTable table = new ObjectTableBuilder()
            .AddColumn(column1Text)
            .AddRow(data1Test)
            .AddRow(data2Test)
            .Build();

        table.Header.Columns.Count.Be(1);
        table.Header.Columns[0].Name.Be(column1Text);

        table.Rows.Count.Be(2);
        table.Rows.All(x => x.Header != null).BeTrue();

        new[] { data1Test, data2Test }
            .Zip(table.Rows, (o, i) => (data: o, row: i))
            .ForEach(x =>
            {
                x.row.Items.Count.Be(1);
                x.row.Items[0].Value.Assert(z => z?.Cast<string>() == x.data);
                x.row.Items[0].Get<string>().Be(x.data);
                x.row.Get<string>(0).Be(x.data);
                x.row.Get<string>(column1Text).Be(x.data);
            });
    }

    [Fact]
    public void ObjectTableSquareTest()
    {
        DateTime now = new DateTime(2023, 4, 1);
        var columns = new[] { "first", "second", "third" };
        var rows = new object[][]
        {
            new object[] { now, "Name 1", 10 },
            new object[] { now.AddDays(1), "Name 2", 10 },
            new object[] { now.AddDays(2), "Name 3", 10 },
        };

        ObjectTable table = new ObjectTableBuilder()
            .AddColumn(columns)
            .AddRow(rows)
            .Build();

        table.Header.Columns.Count.Be(columns.Length);

        table.Header.Columns
            .Zip(columns, (o, i) => (o, i))
            .All(x => x.o.Name == x.i).BeTrue();

        table.Rows.Count.Be(rows.Length);
        table.Rows.All(x => x.Header != null).BeTrue();

        foreach (var row in rows.WithIndex())
        {
            TableRow tableRow = table.Rows[row.Index];
            tableRow.Items.Count.Be(columns.Length);

            tableRow.Get<DateTime>(0).Assert(x => x == (DateTime)row.Item[0]);
            tableRow.Get<string>(1).Be((string)row.Item[1]);
            tableRow.Get<int>(2).Be((int)row.Item[2]);

            tableRow.Get<DateTime>(columns[0]).Assert(x => x == (DateTime)row.Item[0]);
            tableRow.Get<string>(columns[1]).Be((string)row.Item[1]);
            tableRow.Get<int>(columns[2]).Be((int)row.Item[2]);
        }
    }

    [Fact]
    public void ObjectTableTagTest()
    {
        DateTime now = new DateTime(2023, 4, 1);
        var columns = new[] { "first", "second", "third" };
        var rows = new ObjectRow[]
        {
            new ObjectRow( new object[] { now, "Name 1", 10 }, "tag1", "key1"),
            new ObjectRow( new object[] { now.AddDays(1), "Name 2", 10 }),
            new ObjectRow( new object[] { now.AddDays(2), "Name 3", 10 }, "tag2"),
        };

        ObjectTable table = new ObjectTableBuilder()
            .AddColumn(columns)
            .AddRow(rows)
            .Build();

        table.Header.Columns.Count.Be(columns.Length);

        table.Header.Columns
            .Zip(columns, (o, i) => (o, i))
            .All(x => x.o.Name == x.i).BeTrue();

        table.Rows.Count.Be(rows.Length);
        table.Rows.All(x => x.Header != null).BeTrue();

        foreach (var row in rows.WithIndex())
        {
            TableRow tableRow = table.Rows[row.Index];
            tableRow.Items.Count.Be(columns.Length);

            tableRow.Tag.Be(row.Item.Tag);
            tableRow.Key.Be(row.Item.Key);

            tableRow.Get<DateTime>(0).Assert(x => x == (DateTime?)row.Item.Cells[0]);
            tableRow.Get<string>(1).Be((string?)row.Item.Cells[1]);
            tableRow.Get<int>(2).Be((int)row.Item.Cells[2].NotNull());

            tableRow.Get<DateTime>(columns[0]).Assert(x => x == (DateTime?)row.Item.Cells[0]);
            tableRow.Get<string>(columns[1]).Be((string?)row.Item.Cells[1]);
            tableRow.Get<int>(columns[2]).Be((int)row.Item.Cells[2].NotNull());
        }
    }
}