//using System.Collections.Immutable;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public class HubChannelMessageAccess
//{
//    private readonly HubChannelContext _hubContext;
//    private readonly HubChannelClient _client;

//    internal HubChannelMessageAccess(HubChannelContext hubChannelContext, HubChannelClient hubChannelClient)
//    {
//        _hubContext = hubChannelContext.NotNull();
//        _client = hubChannelClient.NotNull();
//    }

//    public async Task<Option<string>> Send(string message, ScopeContext context)
//    {
//        var channelMessage = new ChannelMessageRecord
//        {
//            ChannelId = _hubContext.ChannelId,
//            MessageId = SequenceTool.GenerateId(),
//            Date = DateTime.UtcNow,
//            FromPrincipalId = _hubContext.PrincipalId,
//            Message = message.NotEmpty(),
//        };

//        if (channelMessage.NotNull().Validate().LogStatus(context, "Message valid").IsError(out var r)) return r.ToOptionStatus<string>();

//        var resultOption = await _hubContext.Get(context);
//        if (resultOption.IsError()) return resultOption.ToOptionStatus<string>();

//        var hubChannelRecord = resultOption.Return();
//        if (hubChannelRecord.HasAccess(_hubContext.PrincipalId, ChannelRole.Contributor, context).IsError(out var r1)) return r1.ToOptionStatus<string>();

//        var updateRecord = hubChannelRecord with
//        {
//            Messages = hubChannelRecord.Messages.Append(channelMessage).ToImmutableArray(),
//        };

//        var writeResult = await _client.Set(updateRecord, context);
//        if (writeResult.IsError())
//        {
//            context.LogError("Cannot add message channelId={channelId}, principalId={principalId}");
//            return writeResult.ToOptionStatus<string>();
//        }

//        return channelMessage.MessageId;
//    }

//    public async Task<Option<IReadOnlyList<ChannelMessageRecord>>> Get(ScopeContext context)
//    {
//        var resultOption = await _hubContext.Get(context);
//        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<ChannelMessageRecord>>();

//        var hubChannelRecord = resultOption.Return();
//        if (hubChannelRecord.HasAccess(_hubContext.PrincipalId, ChannelRole.Reader, context).IsError(out var r))
//            return r.ToOptionStatus<IReadOnlyList<ChannelMessageRecord>>();

//        return hubChannelRecord.Messages.ToImmutableArray();
//    }
//}
