using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Connectors;

internal interface ISchemaDataHandler
{
    Task<StatusCode> Delete(ObjectId objectId, ScopeContext context);
    Task<Option<string>> Get(ObjectId objectId, ScopeContext context);
    Task<StatusCode> Set(ObjectId objectId, string payload, ScopeContext context);
}

internal class SchemaDataHandler<TInterface, TModel> : ISchemaDataHandler
    where TInterface : IActorDataBase<TModel>
{
    private readonly IClusterClient _client;
    public SchemaDataHandler(IClusterClient client) => _client = client.NotNull();

    public async Task<StatusCode> Delete(ObjectId objectId, ScopeContext context)
    {
        TInterface actor = _client.GetGrain<TInterface>(objectId);
        return await actor.Delete();
    }

    public async Task<Option<string>> Get(ObjectId objectId, ScopeContext context)
    {
        TInterface actor = _client.GetGrain<TInterface>(objectId);

        return await actor.Get() switch
        {
            { StatusCode: StatusCode.OK } v => v.Return().ToJsonSafe(context.Location()),
            var v => v.ToOption<string>(),
        };
    }

    public async Task<StatusCode> Set(ObjectId objectId, string payload, ScopeContext context)
    {
        TInterface actor = _client.GetGrain<TInterface>(objectId);

        TModel model = payload.ToObject<TModel>().NotNull();
        StatusCode statusCode = await actor.Set(model);
        return statusCode;
    }
}
