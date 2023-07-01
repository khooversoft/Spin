using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MudBlazor;
using SpinCluster.sdk.Actors.Resource;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using SpinPortal.Shared;
using SpinPortal.Shared.Dialogs;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Table;
using Toolbox.Types;

namespace SpinPortal.Pages;

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

    private void Add()
    {
        NavManager.NavigateTo("/tenant/new", true);
        //DialogOptions option = new DialogOptions
        //{
        //    CloseOnEscapeKey = true,
        //    CloseButton = true,
        //    //FullScreen = true,
        //};

        ////DialogParameters parameters = new DialogParameters();
        ////parameters.Add("Title", key);
        ////parameters.Add("TitleTooltip", id.ToString());
        ////parameters.Add("CodeText", json);

        //DialogService.Show<TenantEdit>("Content", option);
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

    private string GetSelectedKey() => _table.Rows[(int)_selectedRow!].Key!;

    private Task Open() => OnOpen(GetSelectedKey());

    private async Task OnOpen(string key)
    {
        //ObjectId id = key.ToObjectId();

        //Option<ResourceFile> document = await Client.Resource.Get(id, new ScopeContext(Logger));
        //if (document.IsError())
        //{
        //    _errorMsg = $"Cannot read file '{id}'";
        //    return;
        //}

        //DialogOptions option = new DialogOptions
        //{
        //    CloseOnEscapeKey = true,
        //    CloseButton = true,
        //    FullScreen = true,
        //};

        //string json = pretty(document.Return().Content.BytesToString());

        //DialogParameters parameters = new DialogParameters();
        //parameters.Add("Title", key);
        //parameters.Add("TitleTooltip", id.ToString());
        //parameters.Add("CodeText", json);

        //DialogService.Show<Code>("Content", parameters, option);

        //static string pretty(string json)
        //{
        //    using var doc = JsonDocument.Parse(json);
        //    return JsonSerializer.Serialize(doc, Json.JsonSerializerFormatOption);
        //}
    }

    private async Task LoadData()
    {
        //var context = new ScopeContext(Logger);

        //try
        //{
        //    var queryParameter = new QueryParameter { Filter = _pathObjectId.ToString() };

        //    Option<IReadOnlyList<StorePathItem>> batch = await Client.Resource.Search(queryParameter, context);
        //    if (batch.IsError())
        //    {
        //        _errorMsg = "Failed to connect to storage";
        //        return;
        //    }

        //    ObjectRow[] rows = batch.Return().Select(x => new ObjectRow(new object?[]
        //        {
        //            x.Name.Split('/').Skip(1).Join('/'),
        //            x.LastModified
        //        }, createTag(x), _pathObjectId.Schema + "/" + x.Name)
        //    ).ToArray();

        //    _table = new ObjectTableBuilder()
        //        .AddColumn(new[]
        //        {
        //                "Name",
        //                "LastModified"
        //        })
        //        .AddRow(rows)
        //        .Build();
        //}
        //catch (OperationCanceledException ex)
        //{
        //    _errorMsg = "Query timed out";
        //    context.Location().LogError(ex, _errorMsg);
        //}
        //finally
        //{
        //    _initialized = true;
        //    _runningQuery = false;
        //    await InvokeAsync(() => StateHasChanged());
        //}

        //string createTag(StorePathItem item) => item.IsDirectory == true ? SpinConstants.Folder : SpinConstants.Open;
    }

    private Task OnRowClick(int? index)
    {
        _selectedRow = index;
        return Task.CompletedTask;
    }
}