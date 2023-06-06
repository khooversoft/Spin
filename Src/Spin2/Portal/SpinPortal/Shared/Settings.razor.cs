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

namespace SpinPortal.Shared;

public partial class Settings
{
    [Parameter]
    public bool Show { get; set; }

    [Parameter]
    public EventCallback<bool> OnSetDark { get; set; }

    private async Task SetDarkMode(bool darkMode)
    {
        await OnSetDark.InvokeAsync(darkMode);
    }
}