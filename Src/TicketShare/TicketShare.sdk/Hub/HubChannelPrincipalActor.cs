using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class HubChannelPrincipalActor
{
    private readonly HubChannelClient _client;
    internal HubChannelPrincipalActor(HubChannelClient hubChannelClient) => _client = hubChannelClient;

    public async Task<Option> Delete(string channelId, string principalId, ScopeContext context)
    {
        channelId.NotEmpty();
        principalId.NotEmpty();

        var hubChannelRecordOption = await _client.Get(channelId, context);
        if (hubChannelRecordOption.IsError()) return hubChannelRecordOption.ToOptionStatus();

        var hubChannelRecord = hubChannelRecordOption.Return();
        if (hubChannelRecord.HasAccess(principalId, ChannelRole.Owner, context).IsError(out var r)) return r;

        if (!hubChannelRecord.Users.ContainsKey(principalId))
        {
            context.LogError("User does not exist in channelId={channelId}, principalId={principalId}", channelId, principalId);
            return StatusCode.NotFound;
        }

        var updateRecord = hubChannelRecord with
        {
            Users = hubChannelRecord.Users
                .ToDictionary()
                .Action(x => x.Remove(principalId))
                .ToFrozenDictionary()
        };

        var writeResult = await _client.Set(updateRecord, context);
        if (writeResult.IsError())
        {
            context.LogError("Cannot delete user channelId={channelId}, principalId={principalId}");
            return writeResult;
        }

        return StatusCode.OK;
    }

    public async Task<Option<ChannelRole>> GetRole(string channelId, string principalId, ScopeContext context)
    {
        channelId.NotEmpty();
        principalId.NotEmpty();

        var resultOption = await _client.Get(channelId, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<ChannelRole>();

        var hubChannelRecord = resultOption.Return();
        if (hubChannelRecord.HasAccess(principalId, ChannelRole.Contributor, context).IsError(out var r)) return r.ToOptionStatus<ChannelRole>();

        if (!hubChannelRecord.Users.TryGetValue(principalId, out var record)) return StatusCode.NotFound;

        return record.Role;
    }

    public async Task<Option<IReadOnlyList<PrincipalChannelModel>>> ListPrincipals(string principalId, string channelId, ScopeContext context)
    {
        channelId.NotEmpty();
        var result = await _client.Get(channelId, context);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<PrincipalChannelModel>>();

        var hubChannelRecord = result.Return();
        if (hubChannelRecord.HasAccess(principalId, ChannelRole.Contributor, context).IsError(out var r)) return r.ToOptionStatus<IReadOnlyList<PrincipalChannelModel>>();

        return hubChannelRecord.Users.Values
            .Select(x => x.ConvertTo())
            .ToImmutableArray();
    }

    public async Task<Option> Set(string ownerPrincipalId, string channelId, string principalId, ChannelRole role, ScopeContext context)
    {
        channelId.NotEmpty();
        ownerPrincipalId.NotEmpty();

        var resultOption = await _client.Get(channelId, context);
        if (resultOption.IsError()) return resultOption.ToOptionStatus();

        HubChannelRecord hubChannelRecord = resultOption.Return();
        if (hubChannelRecord.HasAccess(ownerPrincipalId, ChannelRole.Owner, context).IsError(out var r)) return r;

        if (!hubChannelRecord.Users.TryGetValue(principalId, out var principalChannelRecord)) principalChannelRecord = new PrincipalChannelRecord
        {
            PrincipalId = principalId,
            Role = role
        };

        var updateRecord = hubChannelRecord with
        {
            Users = hubChannelRecord.Users
                .ToDictionary()
                .Action(x => x[principalId] = principalChannelRecord with { Role = role })
                .ToFrozenDictionary(),
        };

        var writeResult = await _client.Set(updateRecord, context);
        if (writeResult.IsError())
        {
            context.LogError("Cannot set user's role channelId={channelId}, principalId={principalId}, role={role}");
            return writeResult;
        }

        return StatusCode.OK;
    }
}
