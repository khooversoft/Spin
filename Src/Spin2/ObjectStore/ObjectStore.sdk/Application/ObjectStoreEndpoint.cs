using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ObjectStore.sdk.Connectors;
using Toolbox.Azure.DataLake;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace ObjectStore.sdk.Application;

internal class ObjectStoreEndpoint
{
    private readonly ObjectStoreConnector _connector;

    public ObjectStoreEndpoint(ObjectStoreConnector api, ILogger<ObjectStoreEndpoint> logger)
    {
        _connector = api.NotNull();
        Logger = logger.NotNull();
    }

    public ILogger<ObjectStoreEndpoint> Logger { get; }

    public async Task<IResult> Read(string objectId, ScopeContext context)
    {
        context = context.With(Logger);
        context.Location().LogInformation("Read objectId={objectId}", objectId);

        ObjectId id = objectId.FromUrlEncoding();
        Option<Document> result = await _connector.Read(id, context);

        return result switch
        {
            var v when v.IsOk() => Results.Ok(v.Return()),
            var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
        };
    }

    public async Task<IResult> Write(Document document, ScopeContext context)
    {
        context = context.With(Logger);
        context.Location().LogInformation("Write document={document}", document);

        var result = await _connector.Write(document, context);

        return result switch
        {
            var v when v.IsOk() => Results.Ok(v.Return()),
            var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
        };
    }

    public async Task<IResult> Delete(string objectId, ScopeContext context)
    {
        context = context.With(Logger);
        context.Location().LogInformation("Delete objectId={objectId}", objectId);

        ObjectId id = objectId.FromUrlEncoding();
        StatusCode result = await _connector.Delete(id, context);

        return Results.StatusCode((int)result.ToHttpStatusCode());
    }

    public async Task<IResult> Search(QueryParameter queryParameter, ScopeContext context)
    {
        context = context.With(Logger);
        context.Location().LogInformation("Search queryParameter={queryParameter}", queryParameter);

        var validatorResult = queryParameter.Validate();
        if (!validatorResult.IsValid)
        {
            return Results.BadRequest(validatorResult.FormatErrors());
        }

        Option<IReadOnlyList<DatalakePathItem>> result = await _connector.Search(queryParameter, context);

        if (result.IsError()) return Results.StatusCode((int)result.StatusCode.ToHttpStatusCode());

        var response = new BatchQuerySet<DatalakePathItem>
        {
            QueryParameter = queryParameter,
            NextIndex = queryParameter.Index + queryParameter.Count,
            Items = result.Return().ToArray(),
        };

        return Results.Ok(response);
    }
}
