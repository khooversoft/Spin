using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Actors.Storage;
using SpinCluster.sdk.Application;
using SpinClusterApi.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Connectors;

internal class SchemaConnector
{
    private readonly ILogger<SchemaConnector> _logger;
    private readonly IReadOnlyDictionary<string, ISchemaDataHandler> _handlers;

    public SchemaConnector(IClusterClient client, ILogger<SchemaConnector> logger)
    {
        _logger = logger.NotNull();

        _handlers = new Dictionary<string, ISchemaDataHandler>()
        {
            [SpinConstants.Schema.User] = new SchemaDataHandler<IUserPrincipleActor, UserPrincipal>(client),
            [SpinConstants.Schema.Key] = new SchemaDataHandler<IPrincipalKeyActor, PrincipalKey>(client),
            [SpinConstants.Schema.Storage] = new SchemaDataHandler<IStorageActor, StorageBlob>(client),
        };
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/data");

        group.MapGet("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) => await Get(objectId, traceId) switch
        {
            var v when v.IsError() => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
            var v when v.HasValue => Results.Ok(v.Return()),
            var v => Results.BadRequest(v.Return()),
        });

        group.MapPost("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId, HttpRequest request) =>
        {
            string body = "";
            using (StreamReader stream = new StreamReader(request.Body))
            {
                body = await stream.ReadToEndAsync();
            }

            StatusCode statusCode = await Set(objectId, traceId, body);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });

        group.MapDelete("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            StatusCode statusCode = await Delete(objectId, traceId);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });
    }

    public async Task<StatusCode> Delete(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = ApiTools.TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.StatusCode;

        if (!_handlers.TryGetValue(oObjectId.Return().Schema, out ISchemaDataHandler? handler)) return StatusCode.BadRequest;

        return await handler.Delete(oObjectId.Return(), context);
    }

    public async Task<Option<object>> Get(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var id = ApiTools.TestObjectId(objectId, context.Location());
        if (id.IsError()) return id.ToOption<string>();

        if (!_handlers.TryGetValue(id.Return().Schema, out ISchemaDataHandler? handler))
            return new Option<object>(StatusCode.BadRequest);

        return await handler.Get(id.Return(), context);
    }

    public async Task<StatusCode> Set(string objectId, string traceId, string payload)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = ApiTools.TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.StatusCode;

        if (!_handlers.TryGetValue(oObjectId.Return().Schema, out ISchemaDataHandler? handler))
            return StatusCode.BadRequest;

        return await handler.Set(oObjectId.Return(), payload, context);
    }
}

