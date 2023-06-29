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

public partial class LeftNav
{
    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    private bool _open = true;
    private string _menuBarIcon = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SetMenuIcon();
    }

    private void ToggleDrawer()
    {
        _open = !_open;
        SetMenuIcon();
    }

    private void SetMenuIcon()
    {
        _menuBarIcon = _open ? Icons.Material.Filled.ChevronLeft : Icons.Material.Filled.ChevronRight;
    }
}