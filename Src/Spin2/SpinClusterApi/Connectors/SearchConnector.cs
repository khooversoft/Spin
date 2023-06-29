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

        app.MapGet("/search/{*filter}", async (
            string filter,
            [FromQuery(Name = "index")] int? index,
            [FromQuery(Name = "count")] int? count,
            [FromQuery(Name = "recurse")] bool? recurse,
            [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId
            ) =>
        {
            var result = await Search(filter, index, count, recurse, traceId);

            return result.IsOk() switch
            {
                true => Results.Ok(result.Return()),
                false => Results.BadRequest($"StatusCode={result.StatusCode}, error={result.Error}"),
            };
        });
    }

    public async Task<Option<IReadOnlyList<StorePathItem>>> Search(string filter, int? index, int? count, bool? recurse, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var query = new SearchQuery
        {
            Index = index ?? 0,
            Count = count ?? 1000,
            Filter = filter,
            Recursive = recurse ?? false,
        };

        ISearchActor actor = _client.GetGrain<ISearchActor>(SpinConstants.SchemaSearch);

        SpinResponse<IReadOnlyList<StorePathItem>> result = await actor.Search(query, context.TraceId);
        if (result.StatusCode.IsError()) return new Option<IReadOnlyList<StorePathItem>>(result.StatusCode, result.Error);

        return result.Return().ToOption();
    }
}
