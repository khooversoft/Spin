using System.Collections.Immutable;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record ChannelInfo
{
    public string ChannelId { get; set; } = null!;
    public string Name { get; init; } = null!;
}


public static class ChannelInfoExtensions
{
    public static async Task<Option<IReadOnlyList<ChannelInfo>>> GetChannelsInfo(this ChannelClient channelClient, string principalId, ScopeContext context)
    {
        var resultOption = await channelClient.NotNull().GetPrincipalChannels(principalId, context).ConfigureAwait(false);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<ChannelInfo>>();

        var result = resultOption.Return();

        var channelInfos = result.Select(x => new ChannelInfo
        {
            ChannelId = x.ChannelId,
            Name = x.Name,
        }).ToImmutableArray();

        return channelInfos;
    }
}