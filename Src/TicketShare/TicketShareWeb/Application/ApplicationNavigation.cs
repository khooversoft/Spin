using Microsoft.AspNetCore.Components;
using Toolbox.Tools;

namespace TicketShareWeb.Application;

public class ApplicationNavigation
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<ApplicationNavigation> _logger;

    public ApplicationNavigation(NavigationManager navigationManager, ILogger<ApplicationNavigation> logger)
    {
        _navigationManager = navigationManager.NotNull();
        _logger = logger.NotNull();
    }

    public void GotoHome() => _navigationManager.NavigateTo("/");

    public void GotoTicketGroups() => _navigationManager.NavigateTo("/TicketGroups");

    public void GotoTicketGroup(string ticketGroupId)
    {
        ticketGroupId.NotEmpty();
        string encoded = $"/TicketGroup/{Uri.EscapeDataString(ticketGroupId)}";
        _navigationManager.NavigateTo(encoded);
    }

    public void GotoChannels() => _navigationManager.NavigateTo("/Channels");

    public void GotoChannel(string channelId)
    {
        channelId.NotEmpty();

        string encoded = $"/Channel/{Uri.EscapeDataString(channelId)}";
        _navigationManager.NavigateTo(encoded);
    }
}
