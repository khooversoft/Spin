using System.Diagnostics;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Directory.Actors;
using SpinCluster.sdk.Directory.Models;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Connectors;

internal class ActorConnector
{
    private readonly ILogger<ActorConnector> _logger;
    private readonly IReadOnlyDictionary<string, ISchemaDataHandler> _handlers;

    public ActorConnector(IClusterClient client, ILogger<ActorConnector> logger)
    {
        _logger = logger.NotNull();

        _handlers = new Dictionary<string, ISchemaDataHandler>()
        {
            [SpinClusterConstants.Schema.User] = new SchemaDataHandler<IUserPrincipleActor, UserPrincipal>(client),
        };
    }

    public async Task<StatusCode> Delete(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.StatusCode;

        if (!_handlers.TryGetValue(oObjectId.Return().Schema, out ISchemaDataHandler? handler)) return StatusCode.BadRequest;

        return await handler.Delete(oObjectId.Return(), context);
    }

    public async Task<Option<string>> Get(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.ToOption<string>();

        if (!_handlers.TryGetValue(oObjectId.Return().Schema, out ISchemaDataHandler? handler))
            return new Option<string>(StatusCode.BadRequest);

        return await handler.Get(oObjectId.Return(), context);
    }

    public async Task<StatusCode> Set(string objectId, string traceId, string payload)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.StatusCode;

        if (!_handlers.TryGetValue(oObjectId.Return().Schema, out ISchemaDataHandler? handler))
            return StatusCode.BadRequest;

        return await handler.Set(oObjectId.Return(), payload, context);
    }

    private static Option<ObjectId> TestObjectId(string objectId, ScopeContextLocation location)
    {
        if (!ObjectId.IsValid(objectId))
        {
            location.LogError("Invalid objectId={objectId}, syntax={syntax}", objectId, ObjectId.Syntax);
            return new Option<ObjectId>($"Invalid objectId={objectId}", StatusCode.BadRequest);
        }

        return objectId.ToObjectId();
    }
}

