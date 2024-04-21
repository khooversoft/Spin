using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Graph;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public interface IDirectoryStoreActor : IGrainWithStringKey
{
    Task<Option<string>> Add(string nodeKey, string name, DataETag value, string traceId);
    Task<Option> Delete(string nodeKey, string name, string traceId);
    Task<Option> Exist(string nodeKey, string name, string traceId);
    Task<Option<DataETag>> Get(string nodeKey, string name, string traceId);
    Task<Option<string>> Set(string nodeKey, string name, DataETag value, string traceId);
}

public class DirectoryStoreActor : Grain, IDirectoryStoreActor
{
    private readonly ILogger<DirectoryStoreActor> _logger;
    private readonly IClusterClient _clusterClient;

    public DirectoryStoreActor(IClusterClient clusterClient, ILogger<DirectoryStoreActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => FileStoreTool.IsPathValid(x), x => $"ActorId={x} is not a valid path");
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<string>> Add(string nodeKey, string name, DataETag value, string traceId)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);

        var addFileOption = await fileStore.Add(value, traceId);
        if (addFileOption.IsError()) return addFileOption;

        string cmd = $"update (key={nodeKey}) set link=-{fileId};";
        var updateResult = await ExecuteScalar(cmd, 1, traceId);

        if (updateResult.StatusCode.IsError())
        {
            await fileStore.Delete(traceId);
            return (StatusCode.Conflict, $"NodeKey={nodeKey} does not exist for update");
        }

        return StatusCode.OK;
    }

    public async Task<Option> Delete(string nodeKey, string name, string traceId)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);

        string cmd = $"update (key={nodeKey}) set link=-{fileId};";
        var updateResult = await ExecuteScalar(cmd, 1, traceId);
        if (updateResult.StatusCode.IsError()) return updateResult.ToOptionStatus();

        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);
        return await fileStore.Delete(traceId);
    }

    public Task<Option> Exist(string nodeKey, string name, string traceId)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);
        return fileStore.Exist(traceId);
    }

    public Task<Option<DataETag>> Get(string nodeKey, string name, string traceId)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);
        return fileStore.Get(traceId);
    }

    public async Task<Option<string>> Set(string nodeKey, string name, DataETag value, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        string fileId = GraphTool.CreateFileId(nodeKey, name);
        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);

        var addFileOption = await fileStore.Set(value, traceId);
        if (addFileOption.IsError()) return addFileOption;

        string cmd = $"update (key={nodeKey}) set link=-{fileId};";
        var updateResult = await ExecuteScalar(cmd, 1, traceId);

        if (updateResult.IsError())
        {
            await fileStore.Delete(traceId);
            context.LogError("NodeKey={nodeKey} does not exist for update", nodeKey);
            return (StatusCode.Conflict, $"NodeKey={nodeKey} does not exist for update");
        }

        context.LogInformation("NodeKey={nodeKey} set", nodeKey);
        return StatusCode.OK;
    }

    private async Task<Option<GraphQueryResult>> ExecuteScalar(string command, int resultCount, string traceId)
    {
        IDirectoryActor directoryActor = _clusterClient.GetDirectory();
        var executeResultOption = await directoryActor.ExecuteScalar(command, traceId);
        if (executeResultOption.IsError()) return executeResultOption;

        var result = resultCount switch
        {
            int when executeResultOption.Return().Items.Count == resultCount => StatusCode.OK,
            _ => StatusCode.NotFound,
        };

        return result;
    }
}
