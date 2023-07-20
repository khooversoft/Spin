using Microsoft.AspNetCore.Components;
using MudBlazor;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Table;
using Toolbox.Types;

namespace SpinPortal.Pages;

public partial class DataEditPage
{
    [Inject] ILogger<DataEditPage> Logger { get; set; } = null!;
    [Inject] PortalOption Option { get; set; } = null!;
    [Inject] SpinClusterClient Client { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;

    [Parameter] public string Schema { get; set; } = null!;
    [Parameter] public string? Tenant { get; set; }
    [Parameter] public string? PageRoute { get; set; } = null!;


    private DisplayState _displayState { get; } = new DisplayState();
    private string? _errorMsg { get; set; }

    private string _title { get; set; } = null!;
    private DataControl _dataControl { get; set; } = new DataControl();
    private ObjectId _dataObjectId { get; set; } = null!;


    private static IReadOnlyDictionary<string, string> _dataTypeInfoDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [SpinConstants.Schema.PrincipalKey] = "Principal Key",
        [SpinConstants.Schema.Tenant] = "Tenant",
        [SpinConstants.Schema.User] = "User",
    };

    protected override async Task OnParametersSetAsync()
    {
        _title = _dataTypeInfoDict[Schema];

        SiloConfigOption siloConfigOption = (await Client.Configuration.Get(new ScopeContext(Logger)))
            .Assert(x => x.StatusCode.IsOk(), "Failed to get Spin configuration from Silo")
            .Return();

        _dataControl.Reset();
        _displayState.Reset();
        _dataControl.Tenants = siloConfigOption.Tenants.ToArray();

        Tenant = Tenant ?? _dataControl.Tenants.FirstOrDefault();
        _dataObjectId = new ObjectId(Schema, Tenant.NotEmpty(), PageRoute);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        lock (_displayState)
        {
            if (_displayState.IsStartUp())
            {
                _ = Task.Run(() => LoadData());
            }
        }
    }

    private void Add() => NavManager.NavigateTo($"{Schema}Edit/{Tenant}", true);
    private void Open() => OnOpen(_dataControl.GetSelectedKey());
    private void OnOpen(string key)
    {
        ObjectId objectId = key.ToObjectId();
        string path = $"{Schema}Edit/{objectId.Tenant}/{objectId.Path}".RemoveTrailing('/');

        NavManager.NavigateTo(path, true);
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
        _displayState.Reset();
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        StateHasChanged();

        await LoadData();
    }


    private async Task LoadData()
    {
        var context = new ScopeContext(Logger);

        try
        {
            var query = new SearchQuery
            {
                Schema = _dataObjectId.Schema,
                Tenant = _dataObjectId.Tenant,
                Filter = _dataObjectId.Path.ToNullIfEmpty() ?? "/",
            };

            Option<ObjectTable> option = await Client.Search.Load(query, context);
            if (option.IsError())
            {
                _errorMsg = option.Error ?? "Query failed";
                return;
            }

            _dataControl.Table = option.Return();
        }
        finally
        {
            _displayState.SetInitialize();
            await InvokeAsync(() => StateHasChanged());
        }
    }

    private Task OnRowClick(int? index)
    {
        _dataControl.SelectIndex = index;
        return Task.CompletedTask;
    }

    private void GotoParent()
    {
        if (_dataObjectId.Paths.Count <= 1) return;

        string path = _dataObjectId.Paths.Take(_dataObjectId.Paths.Count - 1).Join('/');
        string parentPath = $"data/{_dataObjectId.Schema}/{_dataObjectId.Tenant}/{path}";
        NavManager.NavigateTo(parentPath, true);
    }

    private void SetTenant(string tenant) => NavManager.NavigateTo($"data/{Schema}/{Tenant}", true);


    private class DisplayState
    {
        private int _state;
        private enum States { StartUp = 0, RunningQuery, Initialized };

        public void Reset() => Interlocked.Exchange(ref _state, (int)States.StartUp);
        public bool IsStartUp() => Interlocked.CompareExchange(ref _state, (int)States.RunningQuery, (int)States.StartUp) == (int)States.StartUp;
        public void SetInitialize() => Interlocked.Exchange(ref _state, (int)States.Initialized);
        public bool IsInitialized() => _state == (int)States.Initialized;
    }

    private class DataControl
    {
        public ObjectTable? Table { get; set; }
        public int? SelectIndex { get; set; }
        public IReadOnlyList<string> Tenants { get; set; } = Array.Empty<string>();
        public bool DisableRowIcons => SelectIndex == null;
        public bool DisableOpen => SelectIndex == null || Table == null || Table.Rows[(int)SelectIndex].Tag.HasTag(SpinConstants.Folder);
        public bool DisableClear => SelectIndex == null;
        public string GetSelectedKey() => Table?.Rows[(int)SelectIndex!]?.Key ?? "*";

        public void Reset()
        {
            Table = null;
            SelectIndex = null;
        }
    }
}
