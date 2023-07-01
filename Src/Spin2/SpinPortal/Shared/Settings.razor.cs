using Microsoft.AspNetCore.Components;

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