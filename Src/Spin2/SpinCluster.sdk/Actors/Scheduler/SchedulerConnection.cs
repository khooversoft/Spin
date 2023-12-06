using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Scheduler;
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
        group.MapDelete("/{schedulerId}/{principalId}/delete", Delete);
        group.MapGet("/{schedulerId}/schedules", GetSchedules);
        group.MapGet("/{schedulerId}/isWorkAvailable", IsWorkAvailable);

        return group;
    }

    private async Task<IResult> CreateSchedule(ScheduleCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client.GetResourceGrain<ISchedulerActor>(model.SchedulerId).CreateSchedule(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> AssignWork(string schedulerId, string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (agentId.IsEmpty()) return Results.BadRequest();
        schedulerId = Uri.UnescapeDataString(schedulerId);
        if (!ResourceId.IsValid(schedulerId, ResourceType.System, "scheduler")) return Results.BadRequest("Invalid schedulerId");

        Option<WorkAssignedModel> response = await _client.GetResourceGrain<ISchedulerActor>(schedulerId).AssignWork(agentId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Delete(string schedulerId, string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (principalId.IsEmpty()) return Results.BadRequest();
        schedulerId = Uri.UnescapeDataString(schedulerId);
        principalId = Uri.UnescapeDataString(principalId);

        if (!ResourceId.IsValid(schedulerId, ResourceType.System, "scheduler")) return Results.BadRequest("Invalid schedulerId");
        if (!ResourceId.IsValid(principalId, ResourceType.Principal)) return Results.BadRequest("Invalid principalID");

        Option response = await _client.GetResourceGrain<ISchedulerActor>(schedulerId).Delete(principalId, traceId);
        return response.ToResult();
    }

    private async Task<IResult> GetSchedules(string schedulerId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        schedulerId = Uri.UnescapeDataString(schedulerId);
        if (!ResourceId.IsValid(schedulerId, ResourceType.System, "scheduler")) return Results.BadRequest("Invalid schedulerId");

        Option<SchedulesResponseModel> response = await _client.GetResourceGrain<ISchedulerActor>(schedulerId).GetSchedules(traceId);
        return response.ToResult();
    }

    private async Task<IResult> IsWorkAvailable(string schedulerId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        schedulerId = Uri.UnescapeDataString(schedulerId);
        if (!ResourceId.IsValid(schedulerId, ResourceType.System, "scheduler")) return Results.BadRequest("Invalid schedulerId");

        Option response = await _client.GetResourceGrain<ISchedulerActor>(schedulerId).IsWorkAvailable(traceId);
        return response.ToResult();
    }
}
