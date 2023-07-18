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
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Client;
using SpinPortal.Pages.Tenant;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages.PrincipalKey;

public partial class PrincipalKeyEdit
{
    [Inject] public ILogger<TenantEdit> Logger { get; set; } = null!;
    [Inject] NavigationManager NavManager { get; set; } = null!;
    [Inject] public SpinClusterClient Client { get; set; } = null!;

    [Parameter] public string Tenant { get; set; } = null!;
    [Parameter] public string? PageRoute { get; set; }

    private ObjectId _objectId { get; set; } = null!;
    private PrincipalKeyEditModel _model = new PrincipalKeyEditModel();
    private string? _errorMsg { get; set; }
    private string _addOrUpdateButtonText = null!;
    private bool _createMode;
    private string _returnAddress = null!;
    private string _title = null!;

    protected override void OnParametersSet()
    {
        Tenant.NotNull();

        _objectId = new ObjectId("principalKey", Tenant, PageRoute);

        (_title, _addOrUpdateButtonText, _createMode) = _objectId.Path.IsNotEmpty() switch
        {
            true => ("Edit Key", "Update", false),
            false => ("Create Key", "Create", true),
        };

        _returnAddress = $"/data/principalKey/{Tenant}/{_objectId.GetParent()}";
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
        if (success) StateHasChanged();
    }

    private void Cancel() { NavManager.NavigateTo(_returnAddress, true); }

    private async Task<bool> AddOrUpdate()
    {
        //var request = _model.ConvertTo();
        //var requestObjectId = new ObjectId(SpinConstants.Schema.PrincipalKey, SpinConstants.SystemTenant, request.KeyId);

        //Option response = _createMode switch
        //{
        //    true => await Client.PrincipalKey.Create(requestObjectId, request, new ScopeContext(Logger)),
        //    false => await Client.PrincipalKey.Update(requestObjectId, request, new ScopeContext(Logger)),
        //};

        //if (response.StatusCode.IsError())
        //{
        //    _errorMsg = $"Failed to update, statusCode={response.StatusCode}, error={response.Error}";
        //    return false;
        //}

        //NavManager.NavigateTo(_returnAddress, true);
        return true;
    }

    private async Task<PrincipalKeyEditModel> Read()
    {
        if (_createMode) return new PrincipalKeyEditModel
        {
            KeyId = "KeyId1",
            OwnerId = "Key1Owner",
            Name = "Key1Name",
            PrivateKeyExist = true,
        };

        //(PrincipalKeyEditModel result, _errorMsg) = await Client.PrincipalKey.Get(_objectId, new ScopeContext(Logger)) switch
        //{
        //    var v when v.StatusCode.IsError() => (new PrincipalKeyEditModel(), $"Fail to read KeyId={_objectId}, statusCode={v.StatusCode}, error={v.Error}"),
        //    var v => (v.Return().ConvertTo(), null),
        //};

        //return result;
        return null;
    }
}