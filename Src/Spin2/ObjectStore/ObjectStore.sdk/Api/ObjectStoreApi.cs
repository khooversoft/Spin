using Azure;
using Microsoft.Extensions.Logging;
using ObjectStore.sdk.Connectors;
using Toolbox.Azure.DataLake;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace ObjectStore.sdk.Api;

internal class ObjectStoreApi
{
    private readonly ILogger<ObjectStoreApi> _logger;
    private readonly ObjectStoreFactory _factory;

    public ObjectStoreApi(ObjectStoreFactory factory, ILogger<ObjectStoreApi> logger)
    {
        _factory = factory.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<ETag>> Write(Document document, ScopeContext context)
    {
        if (!document.IsVerify()) return new Option<ETag>(StatusCode.BadRequest);

        _logger.LogInformation(context.Location(), "Writing document objectId={objectId}", document.ObjectId);

        (string domain, string path) = document.ObjectId.ToObjectId();
        Option<IDatalakeStore> store = _factory.Get(domain);
        if (store.IsError()) return store.ToOption<ETag>();

        return await store.Return().Write(path, document.ToBytes(), true, context);
    }

    public async Task<Option<Document>> Read(ObjectId objectId, ScopeContext context)
    {
        _logger.LogInformation(context.Location(), "Reading document objectId={objectId}", objectId);

        (string domain, string path) = objectId;
        Option<IDatalakeStore> store = _factory.Get(domain);
        if (store.IsError()) return store.ToOption<Document>();

        Option<DataETag> result = await store.Return().Read(path, context);
        if(result.IsError()) return result.ToOption<Document>();

        return result.Return().Data
            .ToDocument()
            .ToOption();
    }

    public async Task<StatusCode> Delete(ObjectId objectId, ScopeContext context)
    {
        _logger.LogInformation(context.Location(), "Deleting document objectId={objectId}", objectId);

        (string domain, string path) = objectId;
        Option<IDatalakeStore> store = _factory.Get(domain);
        if (store.IsError()) return store.StatusCode;

        return await store.Return().Delete(path, context);
    }
}
