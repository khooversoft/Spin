using Azure;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace ObjectStore.sdk.Connectors;

internal class ObjectStoreConnector
{
    private readonly ILogger<ObjectStoreConnector> _logger;
    private readonly ObjectStoreFactory _factory;

    public ObjectStoreConnector(ObjectStoreFactory factory, ILogger<ObjectStoreConnector> logger)
    {
        _factory = factory.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<ETag>> Write(Document document, ScopeContext context)
    {
        if (!document.Validate().IsValid) return new Option<ETag>(StatusCode.BadRequest);

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
        if (result.IsError()) return result.ToOption<Document>();

        return result.Return().Data.ToDocument();
    }

    public async Task<StatusCode> Delete(ObjectId objectId, ScopeContext context)
    {
        _logger.LogInformation(context.Location(), "Deleting document objectId={objectId}", objectId);

        (string domain, string path) = objectId;
        Option<IDatalakeStore> store = _factory.Get(domain);
        if (store.IsError()) return store.StatusCode;

        return await store.Return().Delete(path, context);
    }

    public async Task<Option<IReadOnlyList<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context)
    {
        _logger.LogInformation(context.Location(), "Searching for queryParameter={queryParameter}", queryParameter);

        Option<IDatalakeStore> store = _factory.Get(queryParameter.Domain.NotEmpty());
        if (store.IsError()) return store.ToOption<IReadOnlyList<DatalakePathItem>>();

        return await store.Return().Search(queryParameter, context);
    }
}
