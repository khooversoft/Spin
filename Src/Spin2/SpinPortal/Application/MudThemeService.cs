using MudBlazor;

namespace SpinPortal.Application;

public class MudThemeService
{
    private const string _drawerWidthRight = "400px";
    private static LayoutProperties _defaultLayoutProperties = new LayoutProperties();

    private MudTheme _mudTheme = new MudTheme
    {
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthRight = _defaultLayoutProperties.DrawerWidthRight,
        },

        Palette = new PaletteLight
        {
            Primary = "#375a7f",
            PrimaryContrastText = Colors.Shades.White,
            Secondary = "#F3F3F3",
            SecondaryContrastText = Colors.Shades.Black,
            Background = PortalConstants.GrayBackgroundColor,
            DrawerBackground = PortalConstants.GrayBackgroundColor,
        },

        PaletteDark = new PaletteDark
        {
            Primary = "#375a7f",
            PrimaryContrastText = Colors.Shades.White,
            Secondary = "#F3F3F3",
            SecondaryContrastText = Colors.Shades.Black,
        }
    };

    public MudTheme Get() => _mudTheme;

    public void SetRightToToolSize() => _mudTheme.LayoutProperties.DrawerWidthRight = _defaultLayoutProperties.DrawerWidthRight;
    public void SetRightToEditSize() => _mudTheme.LayoutProperties.DrawerWidthRight = _drawerWidthRight;
}
