using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using SpinPortal.Shared;
using System.ComponentModel.DataAnnotations;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages.Tenant;

public partial class TenantEdit
{
    [Inject] public ILogger<TenantEdit> Logger { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;

    [Parameter] public string? path { get; set; }

    private ObjectId _objectId { get; set; } = null!;
    private TenantEditModel _model = new TenantEditModel();
    private const string _notActive = "< not active >";
    private string? _errorMsg { get; set; }
    private string _addOrUpdateButtonText = null!;
    private bool _disableUserId;

    protected override async Task OnParametersSetAsync()
    {
        _objectId = path != null ? new ObjectId("tenant", "$system", path) : null!;
        _addOrUpdateButtonText = _objectId != null ? "Update" : "Add";
        _disableUserId = _objectId != null ? true : false;

        _model = await Read();
    }

    private async void OnValidSubmit(EditContext context)
    {
        await AddOrUpdate();
        StateHasChanged();
    }

    private void Cancel() { NavManager.NavigateTo("/tenant"); }

    private async Task AddOrUpdate()
    {
        var request = _model.ConvertTo();

        Option<StatusResponse> result = await Client.Tenant.Set(request.TenantId.ToObjectId("tenant", SpinConstants.SystemTenant), request, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Failed to write, statusCode={result.StatusCode}, error={result.Error}";
        }

        NavManager.NavigateTo("/tenant");
    }

    private async Task<TenantEditModel> Read()
    {
        if (_objectId == null) return new TenantEditModel();

        var result = await Client.Tenant.Get(_objectId, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Fail to read TenantId={_objectId}";
            return new TenantEditModel();
        }

        return result.Return().ConvertTo();
    }
}
