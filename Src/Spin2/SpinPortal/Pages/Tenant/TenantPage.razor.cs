using Microsoft.AspNetCore.Components;
using MudBlazor;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using SpinPortal.Shared;
using Toolbox.Extensions;
using Toolbox.Tools.Table;
using Toolbox.Types;

namespace SpinPortal.Pages.Tenant;

public partial class TenantPage
{
    [Inject] public ILogger<QueryPanel> Logger { get; set; } = null!;
    [Inject] public PortalOption Option { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] public SpinClusterClient SpinClusterClient { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;

    private object _lock = new object();
    private bool _initialized { get; set; }
    private bool _runningQuery { get; set; }
    private string? _errorMsg { get; set; }

    private ObjectTable _table { get; set; } = null!;
    private int? _selectedRow { get; set; }
    private bool _disableRowIcons => _selectedRow == null;
    private bool _disableOpen => _selectedRow == null || !_table.Rows[(int)_selectedRow].Tag.HasTag(SpinConstants.Open);
    private ObjectId _pathObjectId { get; set; } = null!;

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

    private string GetSelectedKey() => _table.Rows[(int)_selectedRow!].Key!;

    private void Add() => NavManager.NavigateTo(PortalConstants.Pages.TenantEditPage(), true);
    private void Open() => OnOpen(GetSelectedKey());
    private void OnOpen(string key)
    {
        ObjectId objectId = key.ToObjectId();
        NavManager.NavigateTo(PortalConstants.Pages.TenantEditPage(objectId.Path), true);
    }


    private async Task Delete()
    {
        //ObjectId id = GetSelectedKey().ToObjectId();

        //StatusCode statusCode = await Client.Resource.Delete(id, new ScopeContext(Logger));
        //if (statusCode.IsError())
        //{
        //    _errorMsg = $"Cannot read file '{id}'";
        //    return;
        //}

        //await Refresh();
        //Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        //Snackbar.Add($"Deleted document, objectId='{id}", Severity.Info);
    }

    private async Task Refresh()
    {
        _initialized = false;
        _selectedRow = null;
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        StateHasChanged();

        await LoadData();
    }


    private async Task LoadData()
    {
        var context = new ScopeContext(Logger);

        try
        {
            var query = new QueryParameter { Filter = "tenant/$system" };
            Option<ObjectTable> option = await Client.Search.Load(query, context);
            if (option.IsError())
            {
                _errorMsg = option.Error ?? "Query failed";
                return;
            }

            _table = option.Return();
        }
        finally
        {
            _initialized = true;
            _runningQuery = false;
            await InvokeAsync(() => StateHasChanged());
        }
    }

    private Task OnRowClick(int? index)
    {
        _selectedRow = index;
        return Task.CompletedTask;
    }
}