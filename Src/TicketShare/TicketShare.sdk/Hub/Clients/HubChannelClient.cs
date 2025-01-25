using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class HubChannelClient
{
    private const string _nodeTag = "hubChannel";
    private const string _hubChannelUserTag = "hubChannel-user";
    private const string _hubTicketGroupUserTag = "ticketGroup-user";
    private readonly ILogger<HubChannelClient> _logger;
    private readonly IGraphClient _graphClient;

    public HubChannelClient(IGraphClient graphClient, IServiceProvider service, ILogger<HubChannelClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> Add(HubChannelRecord identityMessage, ScopeContext context) => AddOrSet(false, identityMessage, context);

    public async Task<Option> Delete(string channelId, ScopeContext context)
    {
        channelId.NotEmpty();
        return await _graphClient.DeleteNode(ToHubChannelId(channelId), context);
    }

    public async Task<Option<HubChannelRecord>> Get(string channelId, ScopeContext context)
    {
        channelId.NotEmpty();
        Option<HubChannelRecord> result = await _graphClient.GetNode<HubChannelRecord>(ToHubChannelId(channelId), context);
        result.LogStatus(context, "Get channelId={channelId}", [channelId]);

        return result;
    }

    public async Task<Option<IReadOnlyList<HubChannelRecord>>> GetByPrincipalId(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey(IdentityClient.ToUserKey(principalId)).SetEdgeType(_hubChannelUserTag))
            .AddRightJoin()
            .AddNodeSearch()
            .AddDataName("entity")
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<HubChannelRecord>>();

        var read = result.Return().DataLinkToObjects<HubChannelRecord>("entity");
        return read.ToOption();
    }

    public Task<Option> Set(HubChannelRecord hubChannelRecord, ScopeContext context) => AddOrSet(true, hubChannelRecord, context);

    private async Task<Option> AddOrSet(bool useSet, HubChannelRecord hubChannelRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (hubChannelRecord.Validate().LogStatus(context, "Record valid").IsError(out var r)) return r.LogStatus(context, nameof(HubChannelRecord));

        string ownerPrincipalId = hubChannelRecord.Users.Values.Where(x => x.Role == ChannelRole.Owner).Single().PrincipalId;
        var userReferences = hubChannelRecord.Users.Values.Where(x => x.Role != ChannelRole.Owner).ToArray();
        string nodeKey = ToHubChannelId(hubChannelRecord.ChannelId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(_nodeTag)
            .AddReferences(_hubChannelUserTag, hubChannelRecord.Users.Values.Select(x => GraphTool.ApplyIfRequired(x.PrincipalId, IdentityClient.ToUserKey)))
            .AddData("entity", hubChannelRecord)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        result.LogStatus(context, "nodeKey={nodeKey}", [nodeKey]);

        if (result.IsError()) return result.LogStatus(context, "nodeKey={nodeKey}", [nodeKey]).ToOptionStatus();

        return result.ToOptionStatus();
    }

    private static string ToUserPrivateHubChannelId(string principalId) => $"hub-channel:{principalId.NotEmpty().ToLowerInvariant()}/private";
    private static string ToHubChannelId(string channelId) => $"hub-channel:{channelId.NotEmpty().ToLowerInvariant()}";
}


public static class HubChannelClientExtensions
{
    public static Task<Option> Add(this HubChannelClient client, string channelId, string ownerPrincipalId, ScopeContext context)
    {
        var model = CreateModel(channelId, ownerPrincipalId);
        return client.Add(model, context);
    }

    private static HubChannelRecord CreateModel(string channelId, string ownerPrincipalId) => new HubChannelRecord
    {
        ChannelId = channelId,
        Users = new Dictionary<string, PrincipalRoleRecord>
        {
            [ownerPrincipalId] = new PrincipalRoleRecord
            {
                PrincipalId = ownerPrincipalId,
                Role = ChannelRole.Owner,
            },
        }.ToFrozenDictionary(),
    };
}