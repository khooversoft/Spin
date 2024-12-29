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
        string uri = $"/TicketGroup/{ticketGroupId}";
        _logger.LogInformation("GotoTicketGroup, uri={uri}", uri);
        _navigationManager.NavigateTo(uri);
    }
}
