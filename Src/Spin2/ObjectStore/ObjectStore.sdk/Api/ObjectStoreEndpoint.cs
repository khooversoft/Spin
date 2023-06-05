using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Toolbox.DocumentContainer;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace ObjectStore.sdk.Api;

internal class ObjectStoreEndpoint
{
    private readonly ILogger<ObjectStoreEndpoint> _logger;
    private readonly ObjectStoreApi _api;

    public ObjectStoreEndpoint(ObjectStoreApi api, ILogger<ObjectStoreEndpoint> logger)
    {
        _api = api.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<IResult> Write(Document document, ScopeContext context)
    {
        var result = await _api.Write(document, context);
        return Results.StatusCode((int)result.StatusCode.ToHttpStatusCode());
    }

    public async Task<IResult> Read(string objectId, ScopeContext context)
    {
        ObjectId id = objectId.FromUrlEncoding();
        Option<Document> result = await _api.Read(id, context);

        return result switch
        {
            var v when v.IsOk() => Results.Ok(v.Value),
            var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
        };
    }

    public async Task<IResult> Delete(string objectId, ScopeContext context)
    {
        ObjectId id = objectId.FromUrlEncoding();
        StatusCode result = await _api.Delete(id, context);

        return Results.StatusCode((int)result.ToHttpStatusCode());
    }
}
