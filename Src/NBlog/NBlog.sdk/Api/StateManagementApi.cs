using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Toolbox.Application;
using Toolbox.Orleans;
using Toolbox.Tools;

namespace NBlog.sdk;

public class StateManagementApi
{
    protected readonly ILogger<StateManagementApi> _logger;
    private readonly StateManagement _stateManagement;

    public StateManagementApi(StateManagement stateManagement, ILogger<StateManagementApi> logger)
    {
        _stateManagement = stateManagement.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/state");

        group.MapGet("/", Get);

        return group;
    }

    private IResult Get(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(Constants.ApiKeyName, out var value)) return Results.Unauthorized();
        if (value != "testKey") return Results.Unauthorized();

        _stateManagement.Clear();
        return Results.Ok();
    }
}
