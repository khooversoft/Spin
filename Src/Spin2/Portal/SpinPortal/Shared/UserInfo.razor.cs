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
using MudBlazor;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpinPortal.Shared;

public partial class UserInfo
{
    [Inject]
    public GraphServiceClient GraphServiceClient { get; set; } = null!;

    [Inject]
    public MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; } = null!;

    [Parameter]
    public bool Show { get; set; }

    private User _user = null!;

    private string UserName { get; set; } = "Not logged on";

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            _user = await GraphServiceClient.Me.Request().GetAsync();
            UserName = _user.DisplayName;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ConsentHandler.HandleException(ex);
        }
    }
}