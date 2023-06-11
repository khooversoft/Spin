using Microsoft.AspNetCore.Components;
using MudBlazor;
using ObjectStore.sdk.Application;
using Toolbox.Tools;
using Toolbox.Tools.Table;
using Toolbox.Extensions;
using Toolbox.Types;
using SpinPortal.Application;

namespace SpinPortal.Shared;

public partial class DataTable
{
    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public ObjectTable Table { get; set; } = null!;

    [Parameter]
    public EventCallback<int?> OnRowClick { get; set; }

    private IReadOnlyList<Column> _columns { get; set; } = Array.Empty<Column>();
    private IReadOnlyList<Row> _rows { get; set; } = Array.Empty<Row>();

    private IReadOnlyList<KeyValuePair<string, string>> _map = Array.Empty<KeyValuePair<string, string>>();
    private int? _selectedRow;

    protected override void OnInitialized()
    {
        Table.NotNull();
        _selectedRow = null;
    }

    protected override void OnParametersSet()
    {
        if (Table == null) return;

        _columns = Table.Header.Columns
            .Select((x, i) => new Column { Title = x.Name, Index = i })
            .ToArray();

        _rows = Table.Rows
            .Select((x, i) => new Row
            {
                Index = i,
                ShowFolder = x.Tag.HasTag(ObjectStoreConstants.Folder),
                ShowOpen = x.Tag.HasTag(ObjectStoreConstants.Open),
                Key = x.Key,

                Items = x.Items
                    .Select(x => x.Get<string>())
                    .ToArray(),

            }).ToArray();

        StateHasChanged();
    }

    private void OnFolderButton(Row row)
    {
        NavManager.NavigateTo(NavTools.ToObjectStorePath(row.Key), true);
    }

    private void OnView(int index)
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

    private async Task OnRowClickInternal(DataGridRowClickEventArgs<Row> e)
    {
        _selectedRow = e.RowIndex;
        await OnRowClick.InvokeAsync(e.RowIndex);
        await InvokeAsync(() => StateHasChanged());
    }

    private string _cellStyleFunc(Row? _) => false switch
    {
        true => "max-width:6rem;padding-right:0;padding-left:0;margin-left:0;margin-right:0",
        false => "max-width:4rem;padding-right:0;padding-left:0",
    };

    private string _rowStyleFunc(Row? row, int index) => _selectedRow == index ? "background-color: #D4D4D4" : string.Empty;

    private string _selectedCellStyleFunc(Row? row) => row.NotNull().Index == _selectedRow ? "color:#000000" : string.Empty;

    private record Column
    {
        public string Title { get; init; } = null!;
        public int Index { get; init; }
    }

    public record Row
    {
        public int Index { get; init; }
        public IReadOnlyList<string> Items { get; init; } = Array.Empty<string>();
        public bool ShowFolder { get; init; }
        public bool ShowOpen { get; init; }
        public string? Key { get; init; }
    }
}
