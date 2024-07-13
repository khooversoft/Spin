using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Actors;

public interface IPartnershipActor : IGrainWithStringKey
{
    Task<Option<PartnershipRecord>> Get(string patnershipId, ScopeContext context);
    Task<Option> Set(PartnershipRecord accountName, ScopeContext context);
}

[StatelessWorker]
public class PartnershipActor : Grain, IPartnershipActor
{
    private readonly ILogger<PartnershipActor> _logger;
    private readonly IClusterClient _clusterClient;

    public PartnershipActor(IClusterClient clusterClient, ILogger<PartnershipActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }


    public async Task<Option<PartnershipRecord>> Get(string patnershipId, ScopeContext context)
    {
        if (patnershipId.IsEmpty()) return StatusCode.BadRequest;
        context = context.With(_logger);

        string command = $"select (key={TicketShareTool.ToPartnershipKey(patnershipId)}) return entity;";
        var resultOption = await _clusterClient.GetDirectoryActor().Execute(command, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, command).ToOptionStatus<PartnershipRecord>();

        var principalIdentity = resultOption.Return().ReturnNames.ReturnNameToObject<PartnershipRecord>("entity");
        return principalIdentity;
    }

    public async Task<Option> Set(PartnershipRecord partnership, ScopeContext context)
    {
        context.With(_logger);
        if (partnership.Validate().LogStatus(context, $"patnershipId={partnership.Id}").IsError(out Option v)) return v;

        // Build graph commands 
        var cmds = new Sequence<string>();
        string base64 = partnership.ToJson64();
        string nodeKey = TicketShareTool.ToPartnershipKey(partnership.Id);

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
