using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

public interface ISchedulerActor : IGrainWithStringKey
{


}

// Actor key = "system:schedule

[StatelessWorker]
[Reentrant]
public class SchedulerActor : Grain, ISchedulerActor
{
    private readonly IPersistentState<SchedulesModel> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SchedulerActor> _logger;

    public SchedulerActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<SchedulesModel> state,
        IClusterClient clusterClient,
        ILogger<SchedulerActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Scheduler, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Schedule(ScheduleWork work, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Schedule work, actorKey={actorKey}, work={work}", this.GetPrimaryKeyString(), work);

        var v = work.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        _state.State = _state.RecordExists ? _state.State : new SchedulesModel();
        _state.State.WorkItems.Enqueue(work);

        await _state.WriteStateAsync();
        return StatusCode.OK;
    }

    //public async Task<Option<ScheduleWork>> GetWork(string agentId, string traceId)
    //{
    //}
}

