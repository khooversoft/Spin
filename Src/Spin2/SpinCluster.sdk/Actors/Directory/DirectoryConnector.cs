//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Routing;
//using Microsoft.Extensions.Logging;
//using SpinCluster.sdk.Actors.Directory;
//using SpinCluster.sdk.Actors.Lease;
//using SpinCluster.sdk.Application;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Actors.Contract;

//public class DirectoryConnector
//{
//    protected readonly IClusterClient _client;
//    protected readonly ILogger<LeaseConnector> _logger;

//    public DirectoryConnector(IClusterClient client, ILogger<LeaseConnector> logger)
//    {
//        _client = client.NotNull();
//        _logger = logger.NotNull();
//    }

//    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
//    {
//        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Directory}");

//        group.MapDelete("/{resourceId}", Delete);
//        group.MapGet("/{resourceId}", Get);
//        group.MapPost("/list", List);
//        group.MapPost("/", Set);

//        return group;
//    }

//    private async Task<IResult> Delete(string resourceId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
//    {
//        resourceId = Uri.UnescapeDataString(resourceId);
//        if (!ResourceId.IsValid(resourceId)) return Results.BadRequest();

//        Option response = await _client.GetDirectoryActor().Delete(resourceId, traceId);
//        return response.ToResult();
//    }

//    private async Task<IResult> Get(string resourceId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
//    {
//        resourceId = Uri.UnescapeDataString(resourceId);
//        if (!ResourceId.IsValid(resourceId)) return Results.BadRequest();

//        var response = await _client.GetDirectoryActor().Get(resourceId, traceId);
//        return response.ToResult();
//    }

//    private async Task<IResult> List(DirectoryQuery query, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
//    {
//        Option<IReadOnlyList<DirectoryEntry>> response = await _client.GetDirectoryActor().List(query, traceId);
//        return response.ToResult();
//    }

//    private async Task<IResult> Set(DirectoryEntry subject, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
//    {
//        var v = subject.Validate();
//        if( v.IsError()) return Results.BadRequest();

//        var response = await _client.GetDirectoryActor().Set(subject, traceId);
//        return response.ToResult();
//    }
//}
