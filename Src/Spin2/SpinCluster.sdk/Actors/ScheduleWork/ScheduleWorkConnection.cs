using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace SpinCluster.sdk.Actors.ScheduleWork;

public class ScheduleWorkConnection
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<ScheduleWorkConnection> _logger;

    public ScheduleWorkConnection(IClusterClient client, ILogger<ScheduleWorkConnection> logger)
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
        if( !ResourceId.IsValid(workId, ResourceType.System, SpinConstants.Schema.ScheduleWork)) Results.BadRequest("Invalid workId");

        Option response = await _client
            .GetResourceGrain<IScheduleWorkActor>(workId)
            .Delete(traceId);

        return response.ToResult();
    }

    private async Task<IResult> Get(string workId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        workId = Uri.UnescapeDataString(workId);
        if( !ResourceId.IsValid(workId, ResourceType.System, SpinConstants.Schema.ScheduleWork)) Results.BadRequest("Invalid workId");

        Option<ScheduleWorkModel> response = await _client
            .GetResourceGrain<IScheduleWorkActor>(workId)
            .Get(traceId);

        return response.ToResult();
    }
}
