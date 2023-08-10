using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages.User;

public partial class UserEdit
{
    [Inject] public ILogger<UserEdit> Logger { get; set; } = null!;
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
        bool success = await AddOrUpdate();
        if(success) StateHasChanged();
    }

    private void Cancel() { NavManager.NavigateTo(_returnAddress, true); }

    private async Task<bool> AddOrUpdate()
    {
        var request = _model.ConvertTo();
        var requestObjectId = new ObjectId(SpinConstants.Schema.User, Tenant, request.UserId);

        //var response = await Client.User.Set(requestObjectId, request, new ScopeContext(Logger));
        //if (response.StatusCode.IsError())
        //{
        //    _errorMsg = $"Failed to write, statusCode={response.StatusCode}, error={response.Error}";
        //    return false;
        //}

        NavManager.NavigateTo(_returnAddress, true);
        return true;
    }

    private async Task<UserEditModel> Read()
    {
        if (_objectId.Path.IsEmpty()) return new UserEditModel();

        //(UserEditModel result, _errorMsg) = await Client.User.Get(_objectId, new ScopeContext(Logger)) switch
        //{
        //    var v when v.StatusCode.IsError() => (new UserEditModel(), $"Fail to read KeyId={_objectId}, statusCode={v.StatusCode}, error={v.Error}"),
        //    var v => (v.Return().ConvertTo(), null),
        //};

        //return result;
        return null;
    }
}
