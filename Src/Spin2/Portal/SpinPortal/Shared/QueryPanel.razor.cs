using Microsoft.AspNetCore.Components;
using ObjectStore.sdk.Client;
using SpinPortal.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Tools.Table;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace SpinPortal.Shared;

public partial class QueryPanel
{
    [Inject]
    public ILogger<QueryPanel> Logger { get; set; } = null!;

    [Inject]
    public PortalOption Option { get; set; } = null!;

    [Inject]
    public ObjectStoreClient Client { get; set; } = null!;

    [Parameter]
    public string Title { get; set; } = null!;

    private object _lock = new object();
    private bool _initialized { get; set; }
    private bool _runningQuery { get; set; }
    private bool _isRefreshRunning => !_initialized;
    private bool _showQuery { get; set; }
    private string? _errorMsg { get; set; }

    private ObjectTable _table { get; set; } = null!;
    private IReadOnlyList<string> _detailColumns { get; set; } = Array.Empty<string>();

    protected override void OnParametersSet()
    {
        _initialized = false;
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
            try
            {
                var queryParameter = new QueryParameter();
                Option<BatchQuerySet<DatalakePathItem>> batch = await Client.Search(queryParameter, new ScopeContext());
                if (batch.IsError())
                {
                    _errorMsg = "Failed to connect to storage";
                    return;
                }

                _table = new ObjectTableBuilder()
                    .AddCoumn(new[]
                    {
                        "Name",
                        "IsDirectory",
                        "LastModified"
                    })
                    .AddRow(batch.Return().Items.Select(x => new object?[]
                    {
                        x.Name,
                        x.IsDirectory,
                        x.LastModified
                    }))
                    .Build();


            }
            finally
            {
                _initialized = true;
                _runningQuery = false;
                await InvokeAsync(() => StateHasChanged());
            }
        }
        catch (OperationCanceledException ex)
        {
            _errorMsg = "Query timed out";
            Logger.LogError(ex, _errorMsg);

            await InvokeAsync(() => StateHasChanged());
        }
    }
}
