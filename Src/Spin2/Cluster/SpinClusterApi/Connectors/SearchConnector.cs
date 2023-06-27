using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using SpinClusterApi.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Connectors;

internal class SearchConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<SearchConnector> _logger;

    public SearchConnector(IClusterClient client, ILogger<SearchConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        // http://{server}/search/{schema}/{tenant}/{path}[/{path}...]?index=n&count=n

        app.MapGet("/search/{*objectId}", async (
            string objectId,
            [FromQuery(Name = "index")] int? index,
            [FromQuery(Name = "count")] int? count,
            [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId
            ) =>
        {
            var result = await Search(objectId, index, count, traceId);

            return result.IsOk() switch
            {
                true => Results.Ok(result.Return()),
                false => Results.StatusCode((int)result.StatusCode.ToHttpStatusCode())
            };
        });
    }

    public async Task<Option<IReadOnlyList<StorePathItem>>> Search(string path, int? index, int? count, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<ObjectId> objectId = ObjectId.CreateIfValid(path);
        if (objectId.IsError())
        {
            context.Location().LogError("ObjectId is not valid, objectId={objectId}");
            return new Option<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest);
        }

        var query = new SearchQuery
        {
            Index = index ?? 0,
            Count = count ?? 1000,
            Filter = path,
        };

        ISearchActor actor = _client.GetGrain<ISearchActor>(objectId.Return());

        SpinResponse<IReadOnlyList<StorePathItem>> result = await actor.Search(query, context);
        if (result.StatusCode.IsError()) return new Option<IReadOnlyList<StorePathItem>>(result.StatusCode);

        return result.Return().ToOption();
    }
}
