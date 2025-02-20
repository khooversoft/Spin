using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace TicketShare.sdk;

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

    public void GotoRegisterExternalLogin(string id) => _navigationManager.NavigateTo($"/RegisterExternalLogin/{id.NotEmpty()}");
}


public static class ApplicationUri
{
    public static string Home => "/";
    public static string TicketGroups => "/TicketGroups";
    public static string Channels => "/Channels";

    public static string GetTicketGroup(string ticketGroupId)
    {
        ticketGroupId.NotEmpty();
        return $"/TicketGroup/{Uri.EscapeDataString(ticketGroupId)}";
    }
    public static string GetChannel(string channelId)
    {
        channelId.NotEmpty();
        return $"/Channel/{Uri.EscapeDataString(channelId)}";
    }
}