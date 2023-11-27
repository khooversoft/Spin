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
        group.MapGet("/{schedulerId}/{agentId}/assign", AssignWork);
        group.MapDelete("/{schedulerId}/{principalId}/clear", Clear);
        group.MapGet("/{schedulerId}/schedules", GetSchedules);

        return group;
    }

    private async Task<IResult> CreateSchedule(ScheduleCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<ISchedulerActor>(model.SchedulerId)
            .CreateSchedule(model, traceId);

        return response.ToResult();
    }

    private async Task<IResult> AssignWork(string schedulerId, string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (agentId.IsEmpty()) return Results.BadRequest();
        schedulerId = Uri.UnescapeDataString(schedulerId);
        if (!ResourceId.IsValid(schedulerId, ResourceType.System, "scheduler")) return Results.BadRequest("Invalid schedulerId");

        Option<WorkAssignedModel> response = await _client
            .GetResourceGrain<ISchedulerActor>(schedulerId)
            .AssignWork(agentId, traceId);

        return response.ToResult();
    }

    private async Task<IResult> Clear(string schedulerId, string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (principalId.IsEmpty()) return Results.BadRequest();
        schedulerId = Uri.UnescapeDataString(schedulerId);
        if (!ResourceId.IsValid(schedulerId, ResourceType.System, "scheduler")) return Results.BadRequest("Invalid schedulerId");

        Option response = await _client
            .GetResourceGrain<ISchedulerActor>(schedulerId)
            .Clear(principalId, traceId);

        return response.ToResult();
    }

    private async Task<IResult> GetSchedules(string schedulerId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        schedulerId = Uri.UnescapeDataString(schedulerId);
        if (!ResourceId.IsValid(schedulerId, ResourceType.System, "scheduler")) return Results.BadRequest("Invalid schedulerId");

        Option<SchedulesResponseModel> response = await _client
            .GetResourceGrain<ISchedulerActor>(schedulerId)
            .GetSchedules(traceId);

        return response.ToResult();
    }
}
