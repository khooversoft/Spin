using Toolbox.Graph.Extensions;
using Toolbox.Tools;

namespace TicketShareWeb.Components.Pages.HomeFeed;

public enum HomeComponentType
{
    None,
    Wellcome,
    Message,
}

public readonly struct HomeComponent
{
    public HomeComponent(HomeComponentType type) => Type = type;

    public HomeComponent(HomeComponentType type, ChannelMessage channelMessage)
    {
        Type = type;
        ChannelMessage = channelMessage.NotNull();
    }

    public HomeComponentType Type { get; }
    public ChannelMessage? ChannelMessage { get; }
    public DateTime? Date => ChannelMessage?.Date;
    public DateTime? Cleared => ChannelMessage?.Cleared;
}

