using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupSearchClient
{
    private const string _nodeTag = "ticketGroup";
    private readonly TicketGroupClient _parent;
    private readonly IGraphClient _graphClient;
    private readonly ILogger _logger;

    internal TicketGroupSearchClient(TicketGroupClient parent, IGraphClient graphClient, ILogger logger)
    {
        _parent = parent.NotNull();
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<IReadOnlyList<TicketGroupRecord>>> GetByOwner(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey(IdentityClient.ToUserKey(principalId)).SetEdgeType("owns"))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(TicketGroupClient._nodeTag))
            .AddDataName("entity")
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, cmd).ToOptionStatus<IReadOnlyList<TicketGroupRecord>>();

        var list = resultOption.Return().DataLinkToObjects<TicketGroupRecord>("entity");
        return list.ToOption();
    }

    public async Task<Option<IReadOnlyList<TicketGroupRecord>>> GetByMember(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey(IdentityClient.ToUserKey(principalId)).SetEdgeType(TicketGroupClient._edgeType))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(_nodeTag))
            .AddDataName("entity")
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, cmd).ToOptionStatus<IReadOnlyList<TicketGroupRecord>>();

        var list = resultOption.Return().DataLinkToObjects<TicketGroupRecord>("entity");
        return list.ToOption();
    }
}
