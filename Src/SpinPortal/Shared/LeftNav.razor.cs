using Microsoft.AspNetCore.Components;
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