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

            SpinResponse<IReadOnlyList<StorePathItem>> result = await Search(query, traceId);

            return result.StatusCode.IsOk() switch
            {
                true => Results.Ok(result.Return()),
                false => Results.BadRequest($"StatusCode={result.StatusCode}, error={result.Error}"),
            };
        });
    }

    public async Task<SpinResponse<IReadOnlyList<StorePathItem>>> Search(SearchQuery query, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        return await _client.GetGrain<ISearchActor>(SpinConstants.SchemaSearch).Search(query, context.TraceId);
    }
}
