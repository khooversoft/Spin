using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using SpinPortal;
using SpinPortal.Shared;
using SpinPortal.Application;
using MudBlazor;
using Toolbox.Extensions;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using static MudBlazor.CategoryTypes;
using Toolbox.Tools.Table;
using Toolbox.Types;
using SpinCluster.sdk.Actors.Search;

namespace SpinPortal.Pages.PrincipalKey;

public partial class PrincipalKeyPage
{
    [Inject] public ILogger<PrincipalKeyPage> Logger { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;

    private object _lock = new object();
    private bool _initialized { get; set; }
    private bool _runningQuery { get; set; }
    private string? _errorMsg { get; set; }

    private ObjectTable _table { get; set; } = null!;
    private int? _selectedRow { get; set; }
    private bool _disableRowIcons => _selectedRow == null;
    private bool _disableOpen => _selectedRow == null || !_table.Rows[(int)_selectedRow].Tag.HasTag(SpinConstants.Open);

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

    private void Add() => NavManager.NavigateTo(PortalConstants.Pages.PrincipalKeyPage(), true);
    private void Open() => OnOpen(GetSelectedKey());
    private void OnOpen(string key)
    {
        ObjectId objectId = key.ToObjectId();
        NavManager.NavigateTo(PortalConstants.Pages.PrincipalKeyPage(objectId.Path), true);
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
            var query = new SearchQuery { Filter = "user/$system" };
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