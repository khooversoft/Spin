using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public class SchedulerConnection
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<ContractConnector> _logger;

    public SchedulerConnection(IClusterClient client, ILogger<ContractConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Scheduler}");

        group.MapPost("create", CreateSchedule);
        group.MapGet("/{agentId}/assign", AssignWork);
        group.MapDelete("/{principalId}/clear", Clear);
        group.MapGet("/schedules", GetSchedules);

        return group;
    }

    private async Task<IResult> CreateSchedule(ScheduleCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .CreateSchedule(model, traceId);

        return response.ToResult();
    }

    private async Task<IResult> AssignWork(string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (agentId.IsEmpty()) return Results.BadRequest();

        Option<WorkAssignedModel> response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .AssignWork(agentId, traceId);

        return response.ToResult();
    }

    private async Task<IResult> Clear(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (principalId.IsEmpty()) return Results.BadRequest();

        Option response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .Clear(principalId, traceId);

        return response.ToResult();
    }

    private async Task<IResult> GetSchedules([FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option<SchedulesModel> response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .GetSchedules(traceId);

        return response.ToResult();
    }
}
