using Microsoft.AspNetCore.Components;
using Toolbox.Tools;

namespace TicketShareWeb.Application;

public class AppNavigation
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<AppNavigation> _logger;

    public AppNavigation(NavigationManager navigationManager, ILogger<AppNavigation> logger)
    {
        _navigationManager = navigationManager.NotNull();
        _logger = logger.NotNull();
    }

    public void GotoTicketGroups()
    {
        _logger.LogInformation("GotoTicketGroups");
        _navigationManager.NavigateTo("/TicketGroups");
    }

    public void GotoTicketGroup(string ticketGroupId)
    {
        ticketGroupId.NotEmpty();

        string uri = $"/TicketGroup/{ticketGroupId}";
        _logger.LogInformation("GotoTicketGroup, uri={uri}", uri);

        string encoded = Uri.EscapeDataString(uri);
        _navigationManager.NavigateTo(encoded);
    }

    public void GotoChannels()
    {
        _logger.LogInformation("GotoChannels");
        _navigationManager.NavigateTo("/Channels");
    }

    public void GotoChannel(string channelId)
    {
        channelId.NotEmpty();

        string uri = $"/TicketGroup/{channelId}";
        _logger.LogInformation("GotoChannel, uri={uri}", uri);

        string encoded = Uri.EscapeDataString(uri);
        _navigationManager.NavigateTo(encoded);
    }
}
