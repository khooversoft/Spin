using Microsoft.AspNetCore.Components;
using MudBlazor;
using ObjectStore.sdk.Application;
using ObjectStore.sdk.Client;
using SpinPortal.Application;
using Toolbox.Azure.DataLake;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Table;
using Toolbox.Types;

namespace SpinPortal.Shared;

public partial class QueryPanel
{
    [Inject] public ILogger<QueryPanel> Logger { get; set; } = null!;
    [Inject] public PortalOption Option { get; set; } = null!;
    [Inject] public ObjectStoreClient Client { get; set; } = null!;
    [Inject] public NavigationManager NavManager { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    [Parameter] public string Title { get; set; } = null!;
    [Parameter] public ObjectUri Path { get; set; } = null!;

    private object _lock = new object();
    private bool _initialized { get; set; }
    private bool _runningQuery { get; set; }
    private string? _errorMsg { get; set; }
    private IReadOnlyList<string> _domains { get; set; } = Array.Empty<string>();

    private ObjectTable _table { get; set; } = null!;
    private int? _selectedRow { get; set; }
    private bool _disableRowIcons => _selectedRow == null;
    private bool _showUpFolderButton => Path.Path.IsNotEmpty();
    private IReadOnlyList<DatalakePathItem> _datalakePathItems = Array.Empty<DatalakePathItem>();

    protected override void OnParametersSet()
    {
        Path.NotNull();

        _initialized = false;
        _domains = Option.Domains.ToArray();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        lock (_lock)
        {
            if (!_initialized && !_runningQuery)
            {
                _runningQuery = true;
                _ = Task.Run(() => LoadData());
            }
        }
    }

    private async Task Refresh()
    {
        _initialized = false;
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        StateHasChanged();

        await LoadData();
    }

    private async Task Open()
    {
        //string file = _datalakePathItems[(int)_selectedRow!].Name.ToObjectId
        //DialogOptions option = new DialogOptions
        //{
        //    Position = DialogPosition.Center,
        //    MaxWidth = MaxWidth.ExtraExtraLarge,
        //    CloseOnEscapeKey = true,
        //    CloseButton = true,
        //};

        //Option<Document> document = await Client.Read(_datalakePathItems[(int)_selectedRow!].Name, new ScopeContext());
        //if (document.IsError())
        //{
        //    _errorMsg = $"Cannot read file '{}'"
        //}

        //DialogParameters parameters = new DialogParameters();
        //parameters.Add("Title", _datalakePathItems[(int)_selectedRow!].Name);
        //parameters.Add("TitleTooltip", "File");
        //parameters.Add("CodeText", "File");

        //DialogService.Show(
        //_initialized = false;
        //await Task.Delay(TimeSpan.FromMilliseconds(500));
        //StateHasChanged();

        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            var queryParameter = new QueryParameter { Domain = Path.Domain, Filter = Path.Path };
            Option<BatchQuerySet<DatalakePathItem>> batch = await Client.Search(queryParameter, new ScopeContext());
            if (batch.IsError())
            {
                _errorMsg = "Failed to connect to storage";
                return;
            }

            _datalakePathItems = batch.Return().Items.ToArray();

            ObjectRow[] rows = batch.Return().Items.Select(x => new ObjectRow(new object?[]
                {
                    x.Name.ToObjectUri().SetDomain(Path.Domain).GetFile(),
                    x.LastModified
                }, getTag(x), x.Name.ToObjectUri().SetDomain(Path.Domain))
            ).ToArray();

            _table = new ObjectTableBuilder()
                .AddColumn(new[]
                {
                        "Name",
                        "LastModified"
                })
                .AddRow(rows)
                .Build();
        }
        catch (OperationCanceledException ex)
        {
            _errorMsg = "Query timed out";
            Logger.LogError(ex, _errorMsg);
        }
        finally
        {
            _initialized = true;
            _runningQuery = false;
            await InvokeAsync(() => StateHasChanged());
        }

        string getTag(DatalakePathItem item) => item.IsDirectory == true ? ObjectStoreConstants.Folder : ObjectStoreConstants.Open;
    }

    private void SetDomain(string domain)
    {
        NavManager.NavigateTo(NavTools.ToObjectStorePath(domain), true);
    }

    private Task OnRowClick(int? index)
    {
        _selectedRow = index;
        return Task.CompletedTask;
    }

    private void GotoParent()
    {
        string parentPath = Path.GetParent();

        NavManager.NavigateTo(NavTools.ToObjectStorePath(parentPath), true);
    }
}
