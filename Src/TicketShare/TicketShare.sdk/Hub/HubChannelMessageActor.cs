using System.Collections.Immutable;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class HubChannelMessageActor
{
    private readonly HubChannelClient _client;
    internal HubChannelMessageActor(HubChannelClient hubChannelClient) => _client = hubChannelClient;

    public async Task<Option> Send(ChannelMessageRecord message, ScopeContext context)
    {
        if (message.NotNull().Validate().LogStatus(context, "Message valid").IsError(out var r)) return r;

        var resultOption = await _client.Get(message.ChannelId, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus();

        var hubChannelRecord = resultOption.Return();
        if (hubChannelRecord.HasAccess(message.FromPrincipalId, ChannelRole.Contributor, context).IsError(out var r1)) return r1;

        var updateRecord = hubChannelRecord with
        {
            Messages = hubChannelRecord.Messages.Append(message).ToImmutableArray(),
        };

        var writeResult = await _client.Set(updateRecord, context);
        if (writeResult.IsError())
        {
            context.LogError("Cannot add message channelId={channelId}, principalId={principalId}");
            return writeResult;
        }
        return StatusCode.OK;
    }

    public async Task<Option<IReadOnlyList<ChannelMessageRecord>>> Get(string principalId, string channelId, ScopeContext context)
    {
        channelId.NotEmpty();

        var resultOption = await _client.Get(channelId, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<ChannelMessageRecord>>();

        var hubChannelRecord = resultOption.Return();
        if (hubChannelRecord.HasAccess(principalId, ChannelRole.Contributor, context).IsError(out var r)) return r.ToOptionStatus<IReadOnlyList<ChannelMessageRecord>>();

        return hubChannelRecord.Messages.ToImmutableArray();
    }
}
