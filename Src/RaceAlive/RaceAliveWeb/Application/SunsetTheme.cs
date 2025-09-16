using MudBlazor;

namespace RaceAliveWeb.Application;

public class SunsetTheme
{
    public static MudTheme Theme
    {
        get
        {
            return new MudTheme
            {
                PaletteLight = new MudBlazor.PaletteLight
                {
                    AppbarBackground = new MudBlazor.Utilities.MudColor("#FD5E53"),
                    Primary = new MudBlazor.Utilities.MudColor("#FD5E53"),
                    Secondary = new MudBlazor.Utilities.MudColor("#FF6B6B"),
                    Tertiary = new MudBlazor.Utilities.MudColor("#FFA07A"),
                    Info = new MudBlazor.Utilities.MudColor("#87CEEB"),
                    Success = new MudBlazor.Utilities.MudColor("#3CB371"),
                    Warning = new MudBlazor.Utilities.MudColor("#DAA520"),
                    Error = new MudBlazor.Utilities.MudColor("#B22222"),
                    Dark = new MudBlazor.Utilities.MudColor("#353839"),
                },
                PaletteDark = new MudBlazor.PaletteDark
                {
                    AppbarBackground = new MudBlazor.Utilities.MudColor("#CC4A43"),
                    Primary = new MudBlazor.Utilities.MudColor("#CC4A43"),
                    Secondary = new MudBlazor.Utilities.MudColor("#CC5656"),
                    Tertiary = new MudBlazor.Utilities.MudColor("#CC8164"),
                    Info = new MudBlazor.Utilities.MudColor("#6B9AC3"),
                    Success = new MudBlazor.Utilities.MudColor("#2C8C59"),
                    Warning = new MudBlazor.Utilities.MudColor("#B2861C"),
                    Error = new MudBlazor.Utilities.MudColor("#8F1B1B"),
                    Dark = new MudBlazor.Utilities.MudColor("#000000"),
                }
            };
        }
    }
}
