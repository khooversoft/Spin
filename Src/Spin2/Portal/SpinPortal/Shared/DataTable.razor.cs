using Microsoft.AspNetCore.Components;
using MudBlazor;
using Toolbox.Tools;
using Toolbox.Tools.Table;

namespace SpinPortal.Shared;

public partial class DataTable
{
    [Parameter]
    public ObjectTable Table { get; set; } = null!;

    [Parameter]
    public bool Show { get; set; } = true;

    [Parameter]
    public bool ShowSearch { get; set; } = true;

    [Parameter]
    public IReadOnlyList<string> DetailColumns { get; set; } = Array.Empty<string>();

    [Parameter]
    public EventCallback<int> OnSearchClick { get; set; }

    private IReadOnlyList<Column> _columns { get; set; } = Array.Empty<Column>();
    private IReadOnlyList<Row> _rows { get; set; } = Array.Empty<Row>();

    private bool _showDetail;
    private IReadOnlyList<KeyValuePair<string, string>> _map = Array.Empty<KeyValuePair<string, string>>();

    protected override void OnInitialized()
    {
        //Table.NotNull();
    }

    protected override void OnParametersSet()
    {
        var dColumns = new HashSet<string>(DetailColumns, StringComparer.OrdinalIgnoreCase);
        if (Table == null) return;

        _columns = Table.Header.Columns
            .Select((x, i) => new Column { Title = x.Name, Index = i })
            .Where(x => !dColumns.Contains(x.Title))
            .ToArray();

        var selectedColumns = new HashSet<int>(_columns.Select(x => x.Index));

        _rows = Table.Rows
            .Select((x, i) => new Row
            {
                Index = i,

                Items = x.Items
                    .Select((x, i) => (value: x.Get<string>(), index: i))
                    .Where(x => selectedColumns.Contains(x.index))
                    .Select(x => x.value)
                    .ToArray(),

            }).ToArray();

        StateHasChanged();
    }

    private async Task OnSearchButton(int index)
    {
        await OnSearchClick.InvokeAsync(index);
    }

    private void OnDetailButton(int index)
    {
        //_map = Table.Rows
        //    .Skip(index)
        //    .SelectMany(x => x.Items)
        //    .Zip(Table.Header.Columns, (o, i) => new KeyValuePair<string, string>(i.Name, o.Get<string>()))
        //    .Where(x => x.Value.IsNotEmpty())
        //    .ToArray();

        //_showDetail = true;
        //StateHasChanged();
    }

    private void OnRowClick(DataGridRowClickEventArgs<Row> e)
    {
        OnDetailButton(e.RowIndex);
    }

    private void OnClose()
    {
        _showDetail = false;
        StateHasChanged();
    }

    private string _cellStyleFunc(Row? _) => ShowSearch switch
    {
        true => "max-width:6rem;padding-right:0;padding-left:0;margin-left:0;margin-right:0",
        false => "max-width:4rem;padding-right:0;padding-left:0",
    };

    private record Column
    {
        public string Title { get; init; } = null!;
        public int Index { get; init; }
    }

    public record Row
    {
        public int Index { get; init; }
        public IReadOnlyList<string> Items { get; init; } = Array.Empty<string>();
    }
}
