using System.Collections.Immutable;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace TicketShare.sdk;

public readonly struct AccountContext
{
    private readonly IGraphClient _graphClient;
    private readonly string _principalId;

    public AccountContext(IGraphClient graphClient, string principalId)
    {
        _graphClient = graphClient.NotNull();
        _principalId = principalId.NotEmpty();

        Messages = new MessagesAccess(Get, Set);
    }

    public MessagesAccess Messages { get; }

    public Task<Option> Add(AccountRecord accountRecord, ScopeContext context) => AddOrSet(false, accountRecord, context);
    public Task<Option> Delete(ScopeContext context) => _graphClient.DeleteNode(AccountTool.ToNodeKey(_principalId), context);
    public Task<Option<AccountRecord>> Get(ScopeContext context) => _graphClient.GetNode<AccountRecord>(AccountTool.ToNodeKey(_principalId), context);
    public Task<Option> Set(AccountRecord accountRecord, ScopeContext context) => AddOrSet(true, accountRecord, context);

    private async Task<Option> AddOrSet(bool useSet, AccountRecord accountRecord, ScopeContext context)
    {
        accountRecord.PrincipalId.Should().Be(_principalId, "PrincipalId does not match");

        var queryOption = AccountTool.CreateQuery(accountRecord, useSet, context);
        if (queryOption.IsError()) return queryOption.ToOptionStatus();

        string cmd = queryOption.Return();

        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        if (result.IsError())
        {
            context.LogError("Failed to set account for principalId={principalId}", accountRecord.PrincipalId);
            return result.LogStatus(context, $"principalId={accountRecord.PrincipalId}").ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    public readonly struct MessagesAccess
    {
        private readonly Func<ScopeContext, Task<Option<AccountRecord>>> _get;
        private readonly Func<AccountRecord, ScopeContext, Task<Option>> _set;
        internal MessagesAccess(Func<ScopeContext, Task<Option<AccountRecord>>> get, Func<AccountRecord, ScopeContext, Task<Option>> set) => (_get, _set) = (get, set);

        public async Task<IReadOnlyList<ChannelMessage>> Get(ScopeContext context)
        {
            var x = (await _get(context).ConfigureAwait(false)) switch
            {
                { StatusCode: StatusCode.OK } v => v.Return().Messages,
                _ => ImmutableList<ChannelMessage>.Empty,
            };

            var current = await _get(context).ConfigureAwait(false);
            if (current.IsError()) return ImmutableList<ChannelMessage>.Empty;
            return current.Return().Messages;
        }

        public async Task<Option> Send(IEnumerable<ChannelMessage> channelMessages, ScopeContext context)
        {
            channelMessages.NotNull();

            var accountRecordOption = await _get(context).ConfigureAwait(false);
            if (accountRecordOption.IsError()) return accountRecordOption.ToOptionStatus();
            var accountRecord = accountRecordOption.Return();

            var messages = accountRecord.Messages.Concat(channelMessages);
            bool emailConfirmed = messages.Any(x => x.FilterType == TsConstants.EmailConfirm);

            if (emailConfirmed) messages = messages.Select(x => clearEmailRequest(x));

            var updatedAccountRecord = accountRecord with
            {
                Messages = messages.ToImmutableArray(),
            };

            return await _set(updatedAccountRecord, context).ConfigureAwait(false);

            ChannelMessage clearEmailRequest(ChannelMessage message) => message switch
            {
                { Cleared: not null } => message,
                { FilterType: TsConstants.EmailRequest } => message with { Cleared = DateTime.UtcNow },
                _ => message,
            };
        }
    }
}
