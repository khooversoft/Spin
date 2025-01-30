using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using Toolbox.Logging;
using System.Collections.Immutable;

namespace Toolbox.Graph.Extensions;

public readonly struct ChannelContext
{
    private readonly IGraphClient _graphClient;
    private readonly string _channelId;
    private readonly string _principalId;
    private readonly ILogger _logger;

    public ChannelContext(IGraphClient graphClient, string channelId, string principalId, ILogger logger)
    {
        _graphClient = graphClient.NotNull();
        _channelId = channelId.NotEmpty();
        _principalId = principalId.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> AddMessage(ChannelMessage message, ScopeContext context) => AddMessages([message], context);

    public async Task<Option> AddMessages(IEnumerable<ChannelMessage> messages, ScopeContext context)
    {
        messages.NotNull();

        var validations = messages
            .Select(x => x.Validate())
            .Aggregate(new Option(StatusCode.OK), (a, x) => a.IsError() ? a : x);

        if (validations.IsError()) return validations;

        var readOption = await GetInternal(SecurityAccess.Contributor, context);
        if (readOption.IsError()) return readOption.ToOptionStatus();

        var subject = readOption.Return();

        var channelRecord = subject with
        {
            Messages = subject.Messages.Concat(messages).ToImmutableArray(),
        };

        var result = await SetInternal(channelRecord, context);
        return result;
    }

    public async Task<Option> Delete(ScopeContext context)
    {
        if ((await GetInternal(SecurityAccess.Contributor, context)).IsError(out var status)) return status.LogStatus(context, "Delete");

        return await _graphClient.DeleteNode(ChannelTool.ToNodeKey(_channelId), context).ConfigureAwait(false);
    }

    public Task<Option<ChannelRecord>> Get(ScopeContext context) => GetInternal(SecurityAccess.Read, context);

    public async Task<Option> Set(ChannelRecord channelRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (channelRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(SecurityGroupRecord));
        if (channelRecord.ChannelId != _channelId) return (StatusCode.Conflict, "SecurityGroupId does not match context");

        var hasAccess = await GetInternal(SecurityAccess.Contributor, context);
        if (!hasAccess.IsNotFound() && hasAccess.IsError(out var r2)) return r2.LogStatus(context, "Set");

        var result = await SetInternal(channelRecord, context);
        return result;
    }

    public async Task<Option<IReadOnlyList<ChannelMessage>>> GetMessages(ScopeContext context)
    {
        var readOption = await GetInternal(SecurityAccess.Read, context);
        if (readOption.IsError()) return readOption.ToOptionStatus<IReadOnlyList<ChannelMessage>>();

        var list = readOption.Return().Messages;
        return list.ToOption();
    }

    private async Task<Option<ChannelRecord>> GetInternal(SecurityAccess accessRequired, ScopeContext context  )
    {
        var subject = await _graphClient.GetNode<ChannelRecord>(ChannelTool.ToNodeKey(_channelId), context).ConfigureAwait(false);
        if (subject.IsError()) return subject;

        var read = subject.Return();
        var securityGroupId = read.SecurityGroupId;

        var hasAccess = await SecurityGroupTool.HasAccess(_graphClient, read.SecurityGroupId, _principalId, accessRequired, context);
        if (hasAccess.IsError()) return hasAccess.ToOptionStatus<ChannelRecord>();

        return subject;
    }

    private async Task<Option> SetInternal(ChannelRecord channelRecord, ScopeContext context)
    {
        var cmdOption = channelRecord.CreateQuery(true, context);
        if (cmdOption.IsError()) return cmdOption.ToOptionStatus();

        var cmd = cmdOption.Return();
        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        result.LogStatus(context, "Set channel, channelId={channelId}", [_channelId]);

        return result.ToOptionStatus();
    }
}
