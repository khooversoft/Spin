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
    private const string _edgeType_owner = "hubChannel-owner";
    private const string _edgeType_user = "hubChannel-user";
    private readonly ILogger<HubChannelClient> _logger;
    private readonly IGraphClient _graphClient;

    public HubChannelClient(IGraphClient graphClient, IServiceProvider service, ILogger<HubChannelClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> Add(HubChannelRecord identityMessage, ScopeContext context) => AddOrSet(false, identityMessage, context);

    public async Task<Option> Delete(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        return await _graphClient.DeleteNode(ToHubChannelId(principalId), context);
    }

    public async Task<Option<HubChannelRecord>> Get(string channelId, ScopeContext context)
    {
        channelId.NotEmpty();
        return await _graphClient.GetNode<HubChannelRecord>(ToHubChannelId(channelId), context);
    }

    public async Task<Option<IReadOnlyList<HubChannelRecord>>> GetByPrincipalId(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetFromKey(IdentityClient.ToUserKey(principalId)).SetEdgeType(_edgeType_owner))
            .AddRightJoin()
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
        if (!hubChannelRecord.Validate(out var r)) return r.LogStatus(context, nameof(HubChannelRecord));

        string ownerPrincipalId = hubChannelRecord.Users.Values.Where(x => x.Role == ChannelRole.Owner).Single().PrincipalId;
        var userReferences = hubChannelRecord.Users.Values.Where(x => x.Role != ChannelRole.Owner).ToArray();
        string nodeKey = ToHubChannelId(hubChannelRecord.ChannelId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            //.AddForeignKeyTag("owns", IdentityClient.ToUserKey(ownerPrincipalId))
            .Action(x => userReferences.ForEach(y =>
            {
                string userKey = IdentityClient.ToUserKey(y.PrincipalId);
                //x.AddForeignKeyTag(ToTagHashKey(userKey), userKey);
            }))
            .AddData("entity", hubChannelRecord)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    private static string ToUserPrivateHubChannelId(string principalId) => $"hub-channel:{principalId.NotEmpty().ToLowerInvariant()}/private";
    private static string ToHubChannelId(string channelId) => $"hub-channel:{channelId.NotEmpty().ToLowerInvariant()}";
}
