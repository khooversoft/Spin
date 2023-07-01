using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Resource;
using SpinCluster.sdk.Types;
using Toolbox.Tools.Zip;
using Toolbox.Tools;
using Toolbox.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using SpinCluster.sdk.Application;
using Microsoft.AspNetCore.Http;
using Toolbox.Azure.DataLake;
using SpinCluster.sdk.Actors.Search;

namespace SpinCluster.sdk.Actors.ActorBase;

public interface IActionOperation<T> : IGrainWithStringKey
{
    Task<SpinResponse<T>> Delete(string traceId);
    Task<SpinResponse<T>> Get(string traceId);
    Task<SpinResponse<T>> Set(T model, string traceId);
    //Task<SpinResponse<QueryResponse<StorePathItem>>> Search(QueryParameter query, string traceId);
}

public abstract class ConnectorBase<T, TActor> where TActor : IActionOperation<T>
{
    private readonly IClusterClient _client;
    private readonly ILogger _logger;
    private readonly string _rootPath;

    public ConnectorBase(IClusterClient client, string rootPath, ILogger logger)
    {
        _client = client.NotNull();
        _rootPath = rootPath.NotEmpty();
        _logger = logger.NotNull();
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"/{_rootPath}");

        group.MapGet("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var response = await Get(objectId, traceId);
            return constructResponse(response);
        });

        group.MapPost("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId, T model) =>
        {
            var response = await Set(objectId, traceId, model);
            return constructResponse(response);
        });

        group.MapDelete("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var response = await Delete(objectId, traceId);
            return constructResponse(response);
        });


        IResult constructResponse(Option<T> option) => option switch
        {
            var v when v.StatusCode == StatusCode.BadRequest => Results.BadRequest(v.ToStatusReponse()),
            var v when v.StatusCode == StatusCode.NotFound => Results.NotFound(v.ToStatusReponse()),
            var v when v.StatusCode == StatusCode.Conflict => Results.Conflict(v.ToStatusReponse()),

            var v when v.IsOk() && v.HasValue => Results.Ok(v.Return()),
            var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
        };
    }

    public async Task<Option<T>> Delete(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) return option.ToOption<T>();

        TActor actor = _client.GetGrain<TActor>(objectId);
        var response = await actor.Delete(context.TraceId);
        return response.ToObject<T>();
    }

    public async Task<Option<T>> Get(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) return option.ToOption<T>();

        TActor actor = _client.GetGrain<TActor>(objectId);
        var response = await actor.Get(context.TraceId);
        return response.ToObject<T>();
    }

    public async Task<Option<T>> Set(string objectId, string traceId, T model)
    {
        var context = new ScopeContext(traceId, _logger);

        var option = objectId.ToObjectIdIfValid(context.Location());
        if (option.IsError()) return option.ToOption<T>();

        TActor actor = _client.GetGrain<TActor>(objectId);
        var response = await actor.Set(model, context.TraceId);
        return response.ToObject<T>();
    }

    public async Task<Option<QueryResponse<StorePathItem>>> Search(string filter, int index, int count, bool recurse, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var query = new SearchQuery
        {
            Index = index,
            Count = count,
            Filter = filter,
            Recurse = recurse,
        };

        ISearchActor actor = _client.GetGrain<ISearchActor>(SpinConstants.SchemaSearch);
        SpinResponse<IReadOnlyList<StorePathItem>> result = await actor.Search(query, context.TraceId);

        return new QueryResponse<StorePathItem>
        {
            Query = query.ConvertTo() with { Index = query.Index + result.Value?.Count ?? 0 },
            Items = result.Value ?? Array.Empty<StorePathItem>(),
            EndOfSearch = result.Value == null || result.Value.Count == 0,
        };
    }
}
