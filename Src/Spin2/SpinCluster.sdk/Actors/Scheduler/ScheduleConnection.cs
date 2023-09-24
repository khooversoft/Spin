using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public class ScheduleConnection
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<ContractConnector> _logger;

    public ScheduleConnection(IClusterClient client, ILogger<ContractConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Scheduler}");

        group.MapPost("runResult", AddResult);
        group.MapPost("enqueue", AddSchedule);
        group.MapGet("/{agentId}/assign", AssignWork);
        group.MapDelete("/{principalId}/clear", Clear);
        group.MapPost("/completed", CompletedWork);
        group.MapGet("/detail", GetDetail);
        group.MapGet("/schedules", GetSchedules);

        return group;
    }

    private async Task<IResult> AddResult(RunResultModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var v = model.Validate();
        if (v.IsError()) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .AddRunResult(model, traceId);

        return response.ToResult();
    }

    private async Task<IResult> AddSchedule(ScheduleCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .AddSchedule(model, traceId);

        return response.ToResult();
    }

    private async Task<IResult> AssignWork(string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (agentId.IsEmpty()) return Results.BadRequest();

        Option<ScheduleWorkModel> response = await _client
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

    private async Task<IResult> CompletedWork(AssignedCompleted model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .CompletedWork(model, traceId);

        return response.ToResult();
    }

    public async Task<IResult> GetDetail(string workId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        workId = Uri.UnescapeDataString(workId);

        Option<ScheduleWorkModel> response = await _client
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .GetDetail(workId, traceId);

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
