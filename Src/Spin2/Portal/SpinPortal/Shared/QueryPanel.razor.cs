using Microsoft.AspNetCore.Components;
using ObjectStore.sdk.Application;
using ObjectStore.sdk.Client;
using SpinPortal.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Tools.Table;
using Toolbox.Types;
using Toolbox.Types.Maybe;
using Toolbox.Extensions;
using Toolbox.Tools;
using System.Linq;

namespace SpinPortal.Shared;

public partial class QueryPanel
{
    [Inject]
    public ILogger<QueryPanel> Logger { get; set; } = null!;

    [Inject]
    public PortalOption Option { get; set; } = null!;

    [Inject]
    public ObjectStoreClient Client { get; set; } = null!;

    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public string Title { get; set; } = null!;

    [Parameter]
    public ObjectUri Path { get; set; } = null!;

    private object _lock = new object();
    private bool _initialized { get; set; }
    private bool _runningQuery { get; set; }
    private string? _errorMsg { get; set; }
    private IReadOnlyList<string> _domains { get; set; } = Array.Empty<string>();

    private ObjectTable _table { get; set; } = null!;
    private int? _selectedRow { get; set; }
    private bool _disableRowIcons => _selectedRow == null;
    private bool _showUpFolderButton => Path.Path.IsNotEmpty();

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
