using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

public class DirectoryConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<DirectoryConnector> _logger;

    public DirectoryConnector(IClusterClient client, ILogger<DirectoryConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Directory}");

        group.MapPost("/command", Command);
        group.MapDelete("/{principalId}/clear", Clear);
        return group;
    }

    private async Task<IResult> Command(DirectoryCommand search, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option<GraphQueryResults> result = await _client.GetDirectoryActor().Execute(search.Command, traceId);
        var response = result.ToResult();
        return response;
    }

    private async Task<IResult> Clear(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (principalId.IsEmpty()) return Results.BadRequest();

        Option response = await _client.GetDirectoryActor().Clear(principalId, traceId);
        return response.ToResult();
    }
}
