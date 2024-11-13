//using System.Collections.Immutable;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public class MessageItemClient
//{
//    private readonly ILogger<MessageItemClient> _logger;
//    private readonly IdentityMessagesClient _client;

//    public MessageItemClient(IdentityMessagesClient client, ILogger<MessageItemClient> logger)
//    {
//        _client = client.NotNull();
//        _logger = logger.NotNull();
//    }

//    public async Task<Option<IReadOnlyList<MessageItemRecord>>> GetMessages(string principalId, bool onlyUnread, ScopeContext context)
//    {
//        principalId.NotEmpty();

//        var readOption = await _client.Get(principalId, context);
//        if (readOption.IsError()) return readOption.ToOptionStatus<IReadOnlyList<MessageItemRecord>>();

//        var read = readOption.Return();
//        var unread = read.Messages.Where(x => !onlyUnread || x.ReadDate == null).ToImmutableArray();

//        return unread;
//    }

//    public async Task<Option> MarkRead(string principalId, string messageId, bool markedRead, ScopeContext context)
//    {
//        principalId.NotEmpty();
//        messageId.NotEmpty();

//        var readOption = await _client.Get(principalId, context);
//        if (readOption.IsError()) return readOption.ToOptionStatus();

//        var identityMessage = readOption.Return();
//        var newIdentityMessage = identityMessage with
//        {
//            Messages = identityMessage.Messages
//                .Select(x => x.MessageId == messageId ? x with { ReadDate = markedRead ? DateTime.UtcNow : null } : x)
//                .ToImmutableArray(),
//        };

//        if (identityMessage == newIdentityMessage) return StatusCode.OK;

//        var result = await _client.Set(newIdentityMessage, context);
//        if (result.IsError()) result.LogStatus(context, "Set failed");

//        return result;
//    }

//    public async Task<Option> Remove(string principalId, string messageId, ScopeContext context)
//    {
//        principalId.NotEmpty();
//        messageId.NotEmpty();

//        var readOption = await _client.Get(principalId, context);
//        if (readOption.IsError()) return readOption.ToOptionStatus();
//        var read = readOption.Return();

//        var newRead = read with
//        {
//            Messages = read.Messages.Where(x => x.MessageId != messageId).ToImmutableArray(),
//        };

//        if (newRead == read) return StatusCode.OK;

//        var result = await _client.Set(newRead, context);
//        return result;
//    }

//    public async Task<Option<string>> Send(string to, string from, string message, string? proposalId, ScopeContext context)
//    {
//        to.NotEmpty();
//        from.NotEmpty();
//        message.NotEmpty();

//        var readOption = await _client.Get(to, context);
//        if (readOption.IsError()) return readOption.ToOptionStatus<string>();
//        var read = readOption.Return();

//        var newMessage = new MessageItemRecord
//        {
//            FromPrincipalId = from,
//            ToPrincipalId = to,
//            Message = message,
//            ProposalId = proposalId,
//        };

//        read = read with
//        {
//            Messages = read.Messages.Append(newMessage).ToImmutableArray(),
//        };

//        var result = await _client.Set(read, context);
//        if (result.IsError()) return result.ToOptionStatus<string>();

//        return newMessage.MessageId;
//    }
//}