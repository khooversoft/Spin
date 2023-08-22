using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

public class UserConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<UserConnector> _logger;

    public UserConnector(IClusterClient client, ILogger<UserConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.User}");

        group.MapDelete("/{principalId}", Delete);
        group.MapGet("/{principalId}", Get);
        group.MapPost("/create", Create);
        group.MapPost("/", Update);
        group.MapPost("/sign", Sign);

        return group;
    }

    private async Task<IResult> Delete(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        principalId = Uri.UnescapeDataString(principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreateUserId(principalId);
        Option response = await _client.GetUserActor(resourceId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        principalId = Uri.UnescapeDataString(principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreateUserId(principalId);
        Option<UserModel> response = await _client.GetUserActor(resourceId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Create(UserCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(_logger);
        var v = model.Validate().LogResult(context.Location());
        if (v.IsError()) return v.ToResult();

        ResourceId resourceId = ResourceId.Create(model.UserId).Return();
        var response = await _client.GetUserActor(resourceId).Create(model, traceId);
        return response.ToResult();
    }

    public async Task<IResult> Update(UserModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate().LogResult(context.Location());
        if (v.IsError()) return v.ToResult();

        ResourceId resourceId = ResourceId.Create(model.UserId).Return();
        var response = await _client.GetUserActor(resourceId).Update(model, context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Sign(SignRequest model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate().LogResult(context.Location());
        if (v.IsError()) return v.ToResult();

        ResourceId resourceId = IdTool.CreateUserId(model.PrincipalId);
        var response = await _client.GetUserActor(resourceId).SignDigest(model.MessageDigest, context.TraceId);
        return response.ToResult();
    }
}
