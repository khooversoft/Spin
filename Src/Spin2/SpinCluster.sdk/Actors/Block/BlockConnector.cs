using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace SpinCluster.sdk.Actors.Block;

public class BlockConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<BlockConnector> _logger;

    public BlockConnector(IClusterClient client, ILogger<BlockConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    //public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    //{
    //    RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.BlockStorage}");

    //    group.MapDelete("/{objectId}", Delete);
    //    group.MapGet("/{objectId}", Get);
    //    group.MapPost("/", Set);

    //    return group;
    //}

    //private async Task<IResult> Delete(string objectId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    Option<ObjectId> option = ObjectId.Create(objectId).LogResult(context.Location());
    //    if (option.IsError()) option.ToResult();

    //    Option response = await _client.GetObjectGrain<IBlockActor>(objectId).Delete(context.TraceId);
    //    return response.ToResult();
    //}

    //public async Task<IResult> Get(string objectId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    Option<ObjectId> option = ObjectId.Create(objectId).LogResult(context.Location());
    //    if (option.IsError()) option.ToResult();

    //    Option<BlobPackage> response = await _client.GetObjectGrain<IBlockActor>(objectId).Get(context.TraceId);
    //    return response.ToResult();
    //}

    //public async Task<IResult> Set(BlobPackage model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    Option<ObjectId> option = ObjectId.Create(model.ObjectId).LogResult(context.Location());
    //    if (option.IsError()) option.ToResult();

    //    var response = await _client.GetObjectGrain<IBlockActor>(option.Return()).Set(model, context.TraceId);
    //    return response.ToResult();
    //}
}
