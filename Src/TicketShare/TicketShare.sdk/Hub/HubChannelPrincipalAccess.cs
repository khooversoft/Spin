//using System.Collections.Frozen;
//using System.Collections.Immutable;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public class HubChannelPrincipalAccess
//{
//    private readonly HubChannelContext _hubContext;
//    private readonly HubChannelClient _client;
//    internal HubChannelPrincipalAccess(HubChannelContext hubChannelContext, HubChannelClient hubChannelClient)
//    {
//        _hubContext = hubChannelContext.NotNull();
//        _client = hubChannelClient.NotNull();
//    }

//    public async Task<Option> Delete(string principalId, ScopeContext context)
//    {
//        principalId.NotEmpty();

//        var hubChannelRecordOption = await _hubContext.Get(context);
//        if (hubChannelRecordOption.IsError()) return hubChannelRecordOption.ToOptionStatus();

//        var hubChannelRecord = hubChannelRecordOption.Return();
//        if (hubChannelRecord.HasAccess(_hubContext.PrincipalId, ChannelRole.Owner, context).IsError(out var r)) return r;

//        if (!hubChannelRecord.Users.ContainsKey(principalId))
//        {
//            context.LogError("User does not exist in channelId={channelId}, principalId={principalId}", _hubContext.ChannelId, principalId);
//            return StatusCode.NotFound;
//        }

//        var updateRecord = hubChannelRecord with
//        {
//            Users = hubChannelRecord.Users
//                .ToDictionary()
//                .Action(x => x.Remove(principalId))
//                .ToFrozenDictionary()
//        };

//        var writeResult = await _client.Set(updateRecord, context);
//        if (writeResult.IsError())
//        {
//            context.LogError("Cannot delete user channelId={channelId}, principalId={principalId}");
//            return writeResult;
//        }

//        return StatusCode.OK;
//    }

//    public async Task<Option<ChannelRole>> GetRole(string principalId, ScopeContext context)
//    {
//        principalId.NotEmpty();

//        var resultOption = await _hubContext.Get(context);
//        if (resultOption.IsError()) return resultOption.ToOptionStatus<ChannelRole>();

//        var hubChannelRecord = resultOption.Return();
//        if (hubChannelRecord.HasAccess(_hubContext.PrincipalId, ChannelRole.Reader, context).IsError(out var r)) return r.ToOptionStatus<ChannelRole>();

//        if (!hubChannelRecord.Users.TryGetValue(principalId, out var record)) return StatusCode.NotFound;

//        return record.Role;
//    }

//    public async Task<Option<IReadOnlyList<PrincipalRoleModel>>> ListPrincipals(ScopeContext context)
//    {
//        var result = await _hubContext.Get(context);
//        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<PrincipalRoleModel>>();

//        var hubChannelRecord = result.Return();
//        if (hubChannelRecord.HasAccess(_hubContext.PrincipalId, ChannelRole.Reader, context).IsError(out var r)) return r.ToOptionStatus<IReadOnlyList<PrincipalRoleModel>>();

//        return hubChannelRecord.Users.Values
//            .Select(x => x.ConvertTo())
//            .ToImmutableArray();
//    }

//    public async Task<Option> Set(string principalId, ChannelRole role, ScopeContext context)
//    {
//        principalId.NotEmpty();

//        var resultOption = await _hubContext.Get(context);
//        if (resultOption.IsError()) return resultOption.ToOptionStatus();

//        HubChannelRecord hubChannelRecord = resultOption.Return();
//        if (hubChannelRecord.HasAccess(_hubContext.PrincipalId, ChannelRole.Owner, context).IsError(out var r)) return r;

//        if (!hubChannelRecord.Users.TryGetValue(principalId, out var principalChannelRecord)) principalChannelRecord = new PrincipalRoleRecord
//        {
//            PrincipalId = principalId,
//            Role = role
//        };

//        var updateRecord = hubChannelRecord with
//        {
//            Users = hubChannelRecord.Users
//                .ToDictionary()
//                .Action(x => x[principalId] = principalChannelRecord with { Role = role })
//                .ToFrozenDictionary(),
//        };

//        var writeResult = await _client.Set(updateRecord, context);
//        if (writeResult.IsError())
//        {
//            context.LogError("Cannot set user's role channelId={channelId}, principalId={principalId}, role={role}");
//            return writeResult;
//        }

//        return StatusCode.OK;
//    }
//}
