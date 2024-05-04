//using Microsoft.Extensions.Logging;
//using Orleans.Concurrency;
//using Toolbox.Graph;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Orleans;

//public interface IDirectoryStoreActor : IGrainWithStringKey
//{
//    Task<Option<string>> Add(string nodeKey, string name, DataETag value, ScopeContext context);
//    Task<Option> Delete(string nodeKey, string name, ScopeContext context);
//    Task<Option> Exist(string nodeKey, string name, ScopeContext context);
//    Task<Option<DataETag>> Get(string nodeKey, string name, ScopeContext context);
//    Task<Option<string>> Set(string nodeKey, string name, DataETag value, ScopeContext context);
//}

//[StatelessWorker]
//public class DirectoryStoreActor : Grain, IDirectoryStoreActor
//{
//    private readonly ILogger<DirectoryStoreActor> _logger;
//    private readonly IClusterClient _clusterClient;

//    public DirectoryStoreActor(IClusterClient clusterClient, ILogger<DirectoryStoreActor> logger)
//    {
//        _clusterClient = clusterClient.NotNull();
//        _logger = logger.NotNull();
//    }

//    public override async Task OnActivateAsync(CancellationToken cancellationToken)
//    {
//        this.GetPrimaryKeyString().Assert(x => FileStoreTool.IsPathValid(x), x => $"ActorId={x} is not a valid path");
//        await base.OnActivateAsync(cancellationToken);
//    }

//    public async Task<Option<string>> Add(string nodeKey, string name, DataETag value, ScopeContext context)
//    {
//        string fileId = GraphTool.CreateFileId(nodeKey, name);
//        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);

//        var addFileOption = await fileStore.Add(value, context);
//        if (addFileOption.IsError()) return addFileOption;

//        string cmd = $"update (key={nodeKey}) set link={fileId};";
//        var updateResult = await ExecuteScalar(cmd, 1, context);

//        if (updateResult.IsError())
//        {
//            await fileStore.Delete(context);
//            return (StatusCode.Conflict, $"NodeKey={nodeKey} does not exist for update");
//        }

//        return StatusCode.OK;
//    }

//    public async Task<Option> Delete(string nodeKey, string name, ScopeContext context)
//    {
//        string fileId = GraphTool.CreateFileId(nodeKey, name);

//        string cmd = $"update (key={nodeKey}) set link=-{fileId};";
//        var updateResult = await ExecuteScalar(cmd, 1, context);
//        if (updateResult.StatusCode.IsError()) return updateResult.ToOptionStatus();

//        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);
//        return await fileStore.Delete(context);
//    }

//    public Task<Option> Exist(string nodeKey, string name, ScopeContext context)
//    {
//        string fileId = GraphTool.CreateFileId(nodeKey, name);
//        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);
//        return fileStore.Exist(context);
//    }

//    public Task<Option<DataETag>> Get(string nodeKey, string name, ScopeContext context)
//    {
//        string fileId = GraphTool.CreateFileId(nodeKey, name);
//        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);
//        return fileStore.Get(context);
//    }

//    public async Task<Option<string>> Set(string nodeKey, string name, DataETag value, ScopeContext context)
//    {
//        context = context.With(_logger);

//        string fileId = GraphTool.CreateFileId(nodeKey, name);
//        IFileStoreActor fileStore = _clusterClient.GetFileStoreActor(fileId);

//        var addFileOption = await fileStore.Set(value, context);
//        if (addFileOption.IsError()) return addFileOption;

//        string cmd = $"update (key={nodeKey}) set link=-{fileId};";
//        var updateResult = await ExecuteScalar(cmd, 1, context);

//        if (updateResult.IsError())
//        {
//            await fileStore.Delete(context);
//            context.LogError("NodeKey={nodeKey} does not exist for update", nodeKey);
//            return (StatusCode.Conflict, $"NodeKey={nodeKey} does not exist for update");
//        }

//        context.LogInformation("NodeKey={nodeKey} set", nodeKey);
//        return StatusCode.OK;
//    }

//    private async Task<Option<GraphQueryResult>> ExecuteScalar(string command, int resultCount, ScopeContext context)
//    {
//        var executeResultOption = await _clusterClient.GetDirectoryActor().ExecuteScalar(command, context);
//        if (executeResultOption.IsError()) return executeResultOption;

//        var result = resultCount switch
//        {
//            int when executeResultOption.Return().Items.Length == resultCount => StatusCode.OK,
//            _ => StatusCode.NotFound,
//        };

//        return result;
//    }
//}
