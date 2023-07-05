using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
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

        app.MapGet("/search", async (
            [FromQuery(Name = "schema")] string schema,
            [FromQuery(Name = "tenant")] string tenant,
            [FromQuery(Name = "filter")] string? filter,
            [FromQuery(Name = "index")] int? index,
            [FromQuery(Name = "count")] int? count,
            [FromQuery(Name = "recurse")] bool? recurse,
            [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId
            ) =>
        {
            var query = new SearchQuery
            {
                Schema = schema,
                Tenant = tenant,
                Filter = filter.ToNullIfEmpty() ?? "/",
                Index = index ?? 0,
                Count = count ?? 1000,
                Recurse = recurse ?? false,
            };

            var result = await Search(query, traceId);

            return result.IsOk() switch
            {
                true => Results.Ok(result.Return()),
                false => Results.BadRequest($"StatusCode={result.StatusCode}, error={result.Error}"),
            };
        });
    }

    public async Task<Option<IReadOnlyList<StorePathItem>>> Search(SearchQuery query, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        ISearchActor actor = _client.GetGrain<ISearchActor>(SpinConstants.SchemaSearch);

        SpinResponse<IReadOnlyList<StorePathItem>> result = await actor.Search(query, context.TraceId);
        if (result.StatusCode.IsError()) return new Option<IReadOnlyList<StorePathItem>>(result.StatusCode, result.Error);

        return result.Return().ToOption();
    }
}
