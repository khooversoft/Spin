using Azure;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using SpinCluster.sdk.Services;
using Toolbox.Azure;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Storage;

public interface IStorageActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<StorageBlob>> Get(string traceId);
    Task<Option> Set(StorageBlob blob, string traceId);
}


public class StorageActor : Grain, IStorageActor
{
    private readonly DatalakeSchemaResources _datalakeSchemaResources;
    private readonly ILogger<StorageActor> _logger;

    public StorageActor(DatalakeSchemaResources datalakeSchemaResources, ILogger<StorageActor> logger)
    {
        _datalakeSchemaResources = datalakeSchemaResources.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        string actorKey = this.GetPrimaryKeyString();
        ResourceId.IsValid(actorKey, ResourceType.DomainOwned).Assert(x => x == true, "Not domain owned format");
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting storage, actorKey={actorKey}", this.GetPrimaryKeyString());

        var schemaOption = GetStore(context);
        if (schemaOption.IsError()) return schemaOption.ToOptionStatus();

        IDatalakeStore store = schemaOption.Return();
        string filePath = VerifyAndGetDetails();

        var result = await store.Delete(filePath, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to delete state file on datalake, actorKey={actorKey}, filePath={filePath}",
                this.GetPrimaryKeyString(), filePath);

            return result;
        }

        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Exist storage, actorKey={actorKey}", this.GetPrimaryKeyString());

        var schemaOption = GetStore(context);
        if (schemaOption.IsError()) return schemaOption.ToOptionStatus();

        IDatalakeStore store = schemaOption.Return();
        string filePath = VerifyAndGetDetails();

        var result = await store.Exist(filePath, context);
        return result;
    }

    public async Task<Option<StorageBlob>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Exist storage, actorKey={actorKey}", this.GetPrimaryKeyString());

        var schemaOption = GetStore(context);
        if (schemaOption.IsError()) return schemaOption.ToOptionStatus<StorageBlob>();

        IDatalakeStore store = schemaOption.Return();
        string filePath = VerifyAndGetDetails();

        var result = await store.Read(filePath, context);
        if (result.IsError())
        {
            context
                .Location()
                .LogWarning("Failed to read file, actorKey={actorKey}, filePath={filePath}", this.GetPrimaryKeyString(), filePath);

            return result.ToOptionStatus<StorageBlob>();
        }

        DataETag dataETag = result.Return();

        var blob = new StorageBlobBuilder()
            .SetStorageId(this.GetPrimaryKeyString())
            .SetETag(dataETag.ETag?.ToString())
            .SetContent(dataETag.Data)
            .Build();

        return blob;
    }

    public async Task<Option> Set(StorageBlob blob, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Exist storage, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!blob.Validate(out var v)) return v;

        bool matchActorKey = blob.StorageId.EqualsIgnoreCase(this.GetPrimaryKeyString());
        if (!matchActorKey) return (StatusCode.BadRequest, $"Storage Id={blob.StorageId} does not match actorKey={this.GetPrimaryKeyString()}");

        var schemaOption = GetStore(context);
        if (schemaOption.IsError()) return schemaOption.ToOptionStatus();

        IDatalakeStore store = schemaOption.Return();
        string filePath = VerifyAndGetDetails();

        var dataEtag = blob.ETag switch
        {
            null => new DataETag(blob.Content),
            string e => new DataETag(blob.Content, new ETag(e))
        };

        var result = await store.Write(filePath, dataEtag, true, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to write state file, actorKey={actorKey}, filePath={filePath}", this.GetPrimaryKeyString(), filePath);
            return result.ToOptionStatus();
        }

        context.Location().LogInformation("Write storage blob to actorKey={actorKey}, filePath={filePath}", this.GetPrimaryKeyString(), filePath);
        return StatusCode.OK;
    }

    private Option<IDatalakeStore> GetStore(ScopeContext context)
    {
        string schema = ResourceId.Create(this.GetPrimaryKeyString()).Return().Schema.NotNull();

        var schemaOption = _datalakeSchemaResources.GetStore(schema);
        if (schemaOption.IsError())
        {
            context.Location().LogError("Cannot get datalake interface for schema={schema}", schema);
            return schemaOption;
        }

        return schemaOption;
    }

    private string VerifyAndGetDetails()
    {
        string actorKey = this.GetPrimaryKeyString();
        ResourceId resourceId = ResourceId.Create(actorKey).Return();
        string filePath = resourceId.BuildPath();

        return filePath;
    }
}
