using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ScheduleWork;

public class ScheduleWorkConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<ScheduleWorkConnector> _logger;

    public ScheduleWorkConnector(IClusterClient client, ILogger<ScheduleWorkConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.ScheduleWork}");

        group.MapPost("/runResult", AddRunResult);
        group.MapPost("/completed", CompletedWork);
        group.MapPost("/create", Create);
        group.MapDelete("/{workId}", Delete);
        group.MapGet("/{workId}", Get);
        group.MapPost("/{workId}/release/{force?}", ReleaseAssign);

        return group;
    }

    private async Task<IResult> AddRunResult(RunResultModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<IScheduleWorkActor>(model.WorkId)
            .AddRunResult(model, traceId);

        return response.ToResult();
    }

    private async Task<IResult> CompletedWork(AssignedCompleted model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<IScheduleWorkActor>(model.WorkId)
            .CompletedWork(model, traceId);

        return response.ToResult();
    }

    private async Task<IResult> Create(ScheduleCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        Option response = await _client
            .GetResourceGrain<IScheduleWorkActor>(model.WorkId)
            .Create(model, traceId);

        return response.ToResult();
    }

    private async Task<IResult> Delete(string workId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        workId = Uri.UnescapeDataString(workId);
        if (!ResourceId.IsValid(workId, ResourceType.System, SpinConstants.Schema.ScheduleWork)) Results.BadRequest("Invalid workId");

        Option response = await _client
            .GetResourceGrain<IScheduleWorkActor>(workId)
            .Delete(traceId);

        return response.ToResult();
    }

    private async Task<IResult> Get(string workId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        workId = Uri.UnescapeDataString(workId);
        if (!ResourceId.IsValid(workId, ResourceType.System, SpinConstants.Schema.ScheduleWork)) Results.BadRequest("Invalid workId");

        Option<ScheduleWorkModel> response = await _client
            .GetResourceGrain<IScheduleWorkActor>(workId)
            .Get(traceId);

        return response.ToResult();
    }

    private async Task<IResult> ReleaseAssign(string workId, bool? force, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        workId = Uri.UnescapeDataString(workId);
        if (!ResourceId.IsValid(workId, ResourceType.System, SpinConstants.Schema.ScheduleWork)) Results.BadRequest("Invalid workId");

        Option response = await _client
            .GetResourceGrain<IScheduleWorkActor>(workId)
            .ReleaseAssign(force ?? false, traceId);

        return response.ToResult();
    }
}
