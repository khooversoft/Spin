using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using SpinPortal.Pages.Tenant;
using SpinPortal.Shared;
using System.ComponentModel.DataAnnotations;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages.User;

public partial class UserEdit
{
    [Inject] public ILogger<TenantEdit> Logger { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;

    [Parameter] public string Tenant { get; set; } = null!;
    [Parameter] public string? PageRoute { get; set; }

    private ObjectId _objectId { get; set; } = null!;
    private UserEditModel _model = new UserEditModel();
    private string? _errorMsg { get; set; }
    private string _addOrUpdateButtonText = null!;
    private bool _disableUserId;
    private string _returnAddress = null!;
    private string _title = null!;

    protected override void OnParametersSet()
    {
        Tenant.NotNull();

        _objectId = new ObjectId("user", Tenant, PageRoute);

        (_title, _addOrUpdateButtonText, _disableUserId) = _objectId.Path.IsNotEmpty() switch
        {
            true => ("Edit User", "Update", true),
            false => ("Add User", "Add", false),
        };

        _returnAddress = $"/data/user/{Tenant}/{_objectId.GetParent()}";
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
        var requestObjectId = new ObjectId(SpinConstants.Schema.User, Tenant, request.UserId);

        Option<StatusResponse> result = await Client.User.Set(requestObjectId, request, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Failed to write, statusCode={result.StatusCode}, error={result.Error}";
        }

        NavManager.NavigateTo(_returnAddress, true);
    }

    private async Task<UserEditModel> Read()
    {
        var result = await Client.User.Get(_objectId, new ScopeContext(Logger));
        if (result.IsError())
        {
            _errorMsg = $"Fail to read TenantId={_objectId}";
            return new UserEditModel();
        }

        return result.Return().ConvertTo();
    }
}
