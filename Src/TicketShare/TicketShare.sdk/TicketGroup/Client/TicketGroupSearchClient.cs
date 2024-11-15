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
    private readonly TicketGroupClient _parent;
    private readonly IGraphClient _graphClient;
    private readonly ILogger<TicketGroupSearchClient> _logger;

    public TicketGroupSearchClient(TicketGroupClient parent, IGraphClient graphClient, ILogger<TicketGroupSearchClient> logger)
    {
        _parent = parent.NotNull();
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<IReadOnlyList<TicketGroupRecord>>> GetByOwner(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey(IdentityClient.ToUserKey(principalId)).SetEdgeType(TicketGroupClient._edgeType))
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
            .AddNodeSearch(x => x.AddTag(TicketGroupClient._nodeTag))
            .AddDataName("entity")
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, cmd).ToOptionStatus<IReadOnlyList<TicketGroupRecord>>();

        var list = resultOption.Return().DataLinkToObjects<TicketGroupRecord>("entity");
        return list.ToOption();
    }
}
