using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Models;
using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Resource;

public interface IResourceActor : IGrainWithStringKey
{
    Task<StatusCode> Delete(string traceId);
    Task<SpinResponse<ResourceFile>> Get(string traceId);
    Task<StatusCode> Set(ResourceFile model, string traceId);
}


[StatelessWorker]
[Reentrant]
public class ResourceActor : Grain, IResourceActor
{
    private readonly DatalakeSchemaResources _datalakeResources;
    private readonly ILogger<SearchActor> _logger;

    public ResourceActor(DatalakeSchemaResources datalakeResources, Validator<SearchQuery> validator, ILogger<SearchActor> logger)
    {
        _datalakeResources = datalakeResources.NotNull();
        _logger = logger.NotNull();
    }

    public virtual async Task<StatusCode> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting id={id}", this.GetPrimaryKeyString());

        Option<ObjectIdInfo> info = ObjectIdInfo.Parse(this.GetPrimaryKeyString(), context);
        if (info.IsError()) return info.StatusCode;

        (ObjectId ObjectId, string FilePath) = info.Return();

        Option<IDatalakeStore> store = _datalakeResources.GetStore(ObjectId.Schema);
        if (store.IsError()) return store.StatusCode;

        return await store.Return().Delete(ObjectId.Path, context);
    }

    public virtual async Task<SpinResponse<ResourceFile>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting id={id}", this.GetPrimaryKeyString());

        Option<ObjectIdInfo> info = ObjectIdInfo.Parse(this.GetPrimaryKeyString(), context);
        if (info.IsError()) return new SpinResponse<ResourceFile>(info.StatusCode, info.Error);

        (ObjectId ObjectId, string FilePath) = info.Return();

        Option<IDatalakeStore> store = _datalakeResources.GetStore(ObjectId.Schema);
        if (store.IsError()) return new SpinResponse<ResourceFile>(StatusCode.BadRequest);

        Option<DataETag> fileData = await store.Return().Read(FilePath, context);
        if (fileData.IsError()) return new SpinResponse<ResourceFile>(StatusCode.BadRequest);

        var result = new ResourceFile
        {
            ObjectId = ObjectId.ToString(),
            Content = fileData.Return().Data,
            ETag = fileData.Return().ETag?.ToString(),
        };

        return new SpinResponse<ResourceFile>(result);
    }

    public virtual async Task<StatusCode> Set(ResourceFile model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Setting id={id}", this.GetPrimaryKeyString());

        Option<ObjectIdInfo> info = ObjectIdInfo.Parse(this.GetPrimaryKeyString(), context);
        if (info.IsError()) return info.StatusCode;

        (ObjectId ObjectId, string FilePath) = info.Return();

        Option<IDatalakeStore> store = _datalakeResources.GetStore(ObjectId.Schema);
        if (store.IsError()) return store.StatusCode;

        var dataEtag = new DataETag(model.Content);
        var result = await store.Return().Write(FilePath, dataEtag, true, context);
        return result.StatusCode;
    }
}
