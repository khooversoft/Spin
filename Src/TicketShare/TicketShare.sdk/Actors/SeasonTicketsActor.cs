using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Actors;

public interface ISeasonTicketsActor : IGrainWithStringKey
{
    Task<Option<SeasonTicketRecord>> Get(string patnershipId, ScopeContext context);
    Task<Option> Set(SeasonTicketRecord accountName, ScopeContext context);
}

[StatelessWorker]
public class SeasonTicketsActor : Grain, ISeasonTicketsActor
{
    private readonly ILogger<SeasonTicketsActor> _logger;
    private readonly IClusterClient _clusterClient;

    public SeasonTicketsActor(IClusterClient clusterClient, ILogger<SeasonTicketsActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }


    public async Task<Option<SeasonTicketRecord>> Get(string patnershipId, ScopeContext context)
    {
        if (patnershipId.IsEmpty()) return StatusCode.BadRequest;
        context = context.With(_logger);

        string command = $"select (key={TicketShareTool.ToSeasonTicketKey(patnershipId)}) return entity;";
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<SeasonTicketRecord>();

        var principalIdentity = resultOption.Return().ReturnNames.ReturnNameToObject<SeasonTicketRecord>("entity");
        return principalIdentity;
    }

    public async Task<Option> Set(SeasonTicketRecord partnership, ScopeContext context)
    {
        context.With(_logger);
        if (partnership.Validate().LogStatus(context, $"patnershipId={partnership.SeasonTicketId}").IsError(out Option v)) return v;

        // Build graph commands 
        var cmds = new Sequence<string>();
        string base64 = partnership.ToJson64();
        string nodeKey = TicketShareTool.ToSeasonTicketKey(partnership.SeasonTicketId);

        //var currentRecordOption = await Get(partnership.Id, context);
        //var updateIndexCmds = currentRecordOption.IsOk() switch
        //{
        //    true => GraphIndexTool.BuildIndexCommands(nodeKey, currentRecordOption.Return().GetIndexKeys(), partnership.GetIndexKeys()),
        //    false => GraphIndexTool.BuildIndexCommands(nodeKey, partnership.GetIndexKeys()),
        //};

        cmds += GraphTool.SetNodeCommand(nodeKey, null, base64, "entity");

        // Create indexes to users, in role

        string command = cmds.Join(Environment.NewLine);
        var result = await _clusterClient.GetDirectoryActor().ExecuteBatch(command, context);
        if (result.IsError()) return result.LogStatus(context, command).ToOptionStatus();

        return StatusCode.OK;
    }
}
