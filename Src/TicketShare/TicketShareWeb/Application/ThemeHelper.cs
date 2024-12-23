using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TicketShareWeb.Application;

public static class ThemeHelper
{
    public static void ApplyTheme(IJSRuntime jsRuntime, bool isDarkMode)
    {
        var baseLayerLuminance = isDarkMode ? StandardLuminance.DarkMode : StandardLuminance.LightMode;
        jsRuntime.InvokeVoidAsync("FluentUI.setBaseLayerLuminance", baseLayerLuminance);
    }
}