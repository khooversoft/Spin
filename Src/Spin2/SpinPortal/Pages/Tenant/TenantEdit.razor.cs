using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages.Tenant;

public partial class TenantEdit
{
    [Inject] public ILogger<TenantEdit> Logger { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;

    [Parameter] public string Tenant { get; set; } = null!;
    [Parameter] public string? PageRoute { get; set; }

    private ObjectId _objectId { get; set; } = null!;
    private TenantEditModel _model = new TenantEditModel();
    private string? _errorMsg { get; set; }
    private string _addOrUpdateButtonText = null!;
    private bool _disableUserId;
    private string _returnAddress = null!;
    private string _title = null!;

    protected override void OnParametersSet()
    {
        Tenant.NotNull();

        _objectId = new ObjectId("tenant", Tenant, PageRoute);

        (_title, _addOrUpdateButtonText, _disableUserId) = _objectId.Path.IsNotEmpty() switch
        {
            true => ("Edit Tenant", "Update", true),
            false => ("Add Tenant", "Add", false),
        };

        _returnAddress = $"/data/tenant/{Tenant}/{_objectId.GetParent()}";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _model = await Read();
            StateHasChanged();
        }
    }

    private async void OnValidSubmit(EditContext context)
    {
        await AddOrUpdate();
        StateHasChanged();
    }

    private void Cancel() { NavManager.NavigateTo(_returnAddress, true); }

    private async Task AddOrUpdate()
    {
        var request = _model.ConvertTo();
        var requestObjectId = new ObjectId(SpinConstants.Schema.Tenant, SpinConstants.SystemTenant, request.TenantId);

        //Option<SpinResponse> result = await Client.Tenant.Set(requestObjectId, request, new ScopeContext(Logger));
        //if (result.IsError())
        //{
        //    _errorMsg = $"Failed to write, statusCode={result.StatusCode}, error={result.Error}";
        //}

        NavManager.NavigateTo(_returnAddress, true);
    }

    private async Task<TenantEditModel> Read()
    {
        if (_objectId == null) return new TenantEditModel();

        var result = await Client.Tenant.Get(_objectId, new ScopeContext(Logger));
        if (result.StatusCode.IsError())
        {
            _errorMsg = $"Fail to read TenantId={_objectId}, statusCode={result.StatusCode}, error={result.Error}";
            return new TenantEditModel();
        }

        return result.Return().ConvertTo();
    }
}
