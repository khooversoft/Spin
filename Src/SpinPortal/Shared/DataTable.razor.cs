using Microsoft.AspNetCore.Components;
using MudBlazor;
using SpinCluster.sdk.Application;
using SpinPortal.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Table;

namespace SpinPortal.Shared;

public partial class DataTable
{
    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public ObjectTable Table { get; set; } = null!;

    [Parameter]
    public EventCallback<int> OnRowClick { get; set; }

    [Parameter]
    public EventCallback<string> OnOpenClick { get; set; }

    private IReadOnlyList<Column> _columns { get; set; } = Array.Empty<Column>();
    private IReadOnlyList<Row> _rows { get; set; } = Array.Empty<Row>();
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
                ShowFolder = x.Tag.HasTag(SpinConstants.Folder),
                ShowOpen = x.Tag.HasTag(SpinConstants.Open),
                Key = x.Key.NotEmpty(),

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

    private async Task OnView(string key)
    {
        await OnOpenClick.InvokeAsync(key);
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
        public string Key { get; init; } = null!;
    }
}
