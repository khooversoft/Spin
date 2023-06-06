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

namespace SpinPortal.Shared;

public partial class MainLayout
{
    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    private bool _isDarkMode;
    private bool _showSettings = false;
    private bool _showUserInfo = false;

    private void GoHome()
    {
        NavManager.NavigateTo("/");
    }

    private void ShowSettings()
    {
        _showSettings = !_showSettings;
        _showUserInfo = _showSettings ? false : _showUserInfo;
    }

    private void ShowUserInfo()
    {
        _showUserInfo = !_showUserInfo;
        _showSettings = _showUserInfo ? false : _showUserInfo;
    }

    MudTheme _mudTheme = new MudTheme
    {
        Palette = new PaletteLight
        {
            Primary = "#375a7f",
            PrimaryContrastText = Colors.Shades.White,
            Secondary = "#F3F3F3",
            SecondaryContrastText = Colors.Shades.Black,
        },

        PaletteDark = new PaletteDark
        {
            Primary = "#375a7f",
            PrimaryContrastText = Colors.Shades.White,
            Secondary = "#F3F3F3",
            SecondaryContrastText = Colors.Shades.Black,
        }
    };
}