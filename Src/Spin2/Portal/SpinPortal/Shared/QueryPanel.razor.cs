using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MudBlazor;
using ObjectStore.sdk.Application;
using ObjectStore.sdk.Client;
using SpinPortal.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Data;
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
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] public JsRunTimeService JsService { get; set; } = null!;

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
    private bool _disableOpen => _selectedRow == null || !_table.Rows[(int)_selectedRow].Tag.HasTag(ObjectStoreConstants.Open);
    private bool _showUpFolderButton => Path.Path.IsNotEmpty();
    private string _uploadStyle => PortalConstants.NormalText; // + ";min-height:36.75px";

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

    private async Task Delete()
    {
        ObjectId id = GetSelectedKey().ToObjectUri().ToObjectId();

        StatusCode result = await Client.Delete(id, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Cannot delete document objectId='{id}'";
            return;
        }

        await Refresh();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        Snackbar.Add($"Deleted document, objectId='{id}", Severity.Info);
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
        ObjectId id = key.ToObjectUri().ToObjectId();

        Option<Document> document = await Client.Read(id, new ScopeContext(Logger));
        if (document.IsError())
        {
            _errorMsg = $"Cannot read file '{id}'";
            return;
        }

        DialogOptions option = new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            FullScreen = true,
        };

        DialogParameters parameters = new DialogParameters();
        parameters.Add("Title", key);
        parameters.Add("TitleTooltip", id.ToString());
        parameters.Add("CodeText", document.Return().ToObject<string>());

        DialogService.Show<Code>("Content", parameters, option);
    }

    private async Task OnUploadFile(InputFileChangeEventArgs args)
    {
        var memoryStream = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(memoryStream);

        string fileName = args.File.Name
            .Select(x => char.IsLetterOrDigit(x) || x == '-' || x == '.' ? x : '-')
            .Func(x => new string(x.ToArray()));

        ObjectId id = (Path.Id + "/" + fileName).ToObjectUri().ToObjectId();

        var document = new DocumentBuilder()
            .SetDocumentId(id)
            .SetContent(memoryStream.ToArray())
            .Build();

        await Client.Write(document, new ScopeContext(Logger));
    }

    private async Task Download()
    {
        ObjectUri path = GetSelectedKey().ToObjectUri();
        ObjectId id = path.ToObjectId();

        Option<Document> document = await Client.Read(id, new ScopeContext(Logger));
        if (document.IsError())
        {
            _errorMsg = $"Cannot read file '{id}'";
            return;
        }

        await JsService.DownloadFile(path.GetFile().NotEmpty(), document.Return().Content);
    }

    private async Task LoadData()
    {
        var context = new ScopeContext(Logger);

        try
        {
            var queryParameter = new QueryParameter { Domain = Path.Domain, Filter = Path.Path };

            Option<BatchQuerySet<DatalakePathItem>> batch = await Client.Search(queryParameter, context);
            if (batch.IsError())
            {
                _errorMsg = "Failed to connect to storage";
                return;
            }

            ObjectRow[] rows = batch.Return().Items.Select(x => new ObjectRow(new object?[]
                {
                    x.Name.ToObjectUri().SetDomain(Path.Domain).GetFile(),
                    x.LastModified
                }, createTag(x), x.Name.ToObjectUri().SetDomain(Path.Domain))
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
            context.Location().LogError(ex, _errorMsg);
        }
        finally
        {
            _initialized = true;
            _runningQuery = false;
            await InvokeAsync(() => StateHasChanged());
        }

        string createTag(DatalakePathItem item) => item.IsDirectory == true ? ObjectStoreConstants.Folder : ObjectStoreConstants.Open;
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
