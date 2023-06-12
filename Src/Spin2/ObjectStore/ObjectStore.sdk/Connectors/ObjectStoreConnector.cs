using Azure;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

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
        context = context.With(_logger);
        if (!document.Validate().IsValid) return new Option<ETag>(StatusCode.BadRequest);

        context.Location().LogInformation("Writing document objectId={objectId}", document.ObjectId);

        (string domain, string path) = document.ObjectId.ToObjectId();
        Option<IDatalakeStore> store = _factory.Get(domain);
        if (store.IsError()) return store.ToOption<ETag>();

        return await store.Return().Write(path, document.ToBytes(), true, context);
    }

    public async Task<Option<Document>> Read(ObjectId objectId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Reading document objectId={objectId}", objectId);

        (string domain, string path) = objectId;
        Option<IDatalakeStore> store = _factory.Get(domain);
        if (store.IsError()) return store.ToOption<Document>();

        Option<DataETag> result = await store.Return().Read(path, context);
        if (result.IsError()) return result.ToOption<Document>();

        var document = new DocumentBuilder()
            .SetDocumentId(objectId)
            .SetContent(result.Return().Data)
            .Build();

        return document;
    }

    public async Task<StatusCode> Delete(ObjectId objectId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Deleting document objectId={objectId}", objectId);

        (string domain, string path) = objectId;
        Option<IDatalakeStore> store = _factory.Get(domain);
        if (store.IsError()) return store.StatusCode;

        return await store.Return().Delete(path, context);
    }

    public async Task<Option<IReadOnlyList<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Searching for queryParameter={queryParameter}", queryParameter);

        Option<IDatalakeStore> store = _factory.Get(queryParameter.Domain.NotEmpty());
        if (store.IsError()) return store.ToOption<IReadOnlyList<DatalakePathItem>>();

        return await store.Return().Search(queryParameter, context);
    }
}
