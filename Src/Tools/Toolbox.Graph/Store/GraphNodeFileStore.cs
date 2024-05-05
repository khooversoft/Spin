//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GraphNodeFileStore : IGraphNodeFileStore
//{
//    private readonly IGraphTrxContext _graphTrxContext;
//    private readonly ILogger<GraphNodeFileStore> _logger;

//    public GraphNodeFileStore(IGraphTrxContext graphTrxContext, ILogger<GraphNodeFileStore> logger)
//    {
//        _graphTrxContext = graphTrxContext.NotNull();
//        _logger = logger.NotNull();
//    }

//    public async Task<Option<string>> Add(string nodeKey, string name, DataETag data, ScopeContext context)
//    {
//        string fileId = GraphTool.CreateFileId(nodeKey, name);

//        var currentOption = await Get(nodeKey, name, context);
//        if (currentOption.IsOk())

//            return result;
//    }

//    //    private readonly GraphMemoryContext _graphDbContext;
//    //    private readonly IFileStore _graphFileStore;

//    //    internal GraphStoreMemory(GraphMemoryContext graphDbContext, IFileStore graphFileStore)
//    //    {
//    //        _graphDbContext = graphDbContext.NotNull();
//    //        _graphFileStore = graphFileStore.NotNull();
//    //    }

//    //    public async Task<Option<string>> Add(string nodeKey, string name, DataETag data, ScopeContext context)
//    //    {
//    //        var result = await AddOrUpdate(nodeKey, name, false, data, context);
//    //        return result;
//    //    }

//    //    public async Task<Option> Delete(string nodeKey, string name, ScopeContext context)
//    //    {
//    //        string fileId = GraphTool.CreateFileId(nodeKey, name);

//    //        string cmd = $"update (key={nodeKey}) set link=-{fileId};";
//    //        var updateResult = await _graphDbContext.Command.ExecuteScalar(cmd, context);
//    //        if (updateResult.StatusCode.IsError()) return updateResult.ToOptionStatus();
//    //        if (updateResult.Return().Items.Length == 0) return StatusCode.NotFound;

//    //        using (var scope = await _graphDbContext.ReadWriterLock.ReaderLockAsync())
//    //        {
//    //            await _graphDbContext.WriteMapToStore(context);
//    //        }

//    //        return await _graphFileStore.Delete(fileId, context);
//    //    }

//    //    public Task<Option> Exist(string nodeKey, string name, ScopeContext context)
//    //    {
//    //        string fileId = GraphTool.CreateFileId(nodeKey, name);
//    //        return _graphFileStore.Exist(fileId, context);
//    //    }

//    public async Task<Option<DataETag>> Get(string nodeKey, string name, ScopeContext context)
//    {
//        string fileId = GraphTool.CreateFileId(nodeKey, name);
//        var dataOption = await _graphTrxContext.FileStore.Get(fileId, context);
//        if (dataOption.IsError()) return dataOption;

//        return dataOption.Return();
//    }

//    //    public async Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
//    //    {
//    //        var result = await _graphFileStore.Search(pattern, context);
//    //        return result;
//    //    }

//    //    public async Task<Option<string>> Set(string nodeKey, string name, DataETag data, ScopeContext context)
//    //    {
//    //        var result = await AddOrUpdate(nodeKey, name, true, data, context);
//    //        return result;
//    //    }

//    private async Task<Option<string>> AddOrUpdate(string nodeKey, string name, bool upsert, DataETag data, ScopeContext context)
//    {
//        string fileId = GraphTool.CreateFileId(nodeKey, name);

//        var currentOption = await Get(nodeKey, name, context);
//        if( currentOption.IsOk())

//        var addOption = upsert switch
//        {
//            false => await _graphTrxContext.FileStore.Add(fileId, data, context),
//            true => await _graphTrxContext.FileStore.Set(fileId, data, context),
//        };

//        if (addOption.IsError()) return addOption;

//        string cmd = $"update (key={nodeKey}) set link={fileId};";
//        var updateResult = await _graphDbContext.Command.ExecuteScalar(cmd, context);
//        if (updateResult.StatusCode.IsError() || updateResult.Return().Items.Length == 0)
//        {
//            await _graphTrxContext.Delete(fileId, context);
//            return (StatusCode.Conflict, $"NodeKey={nodeKey} does not exist for update");
//        }

//        using (var release = await _graphDbContext.Map.ReadWriterLock.ReaderLockAsync())
//        {
//            await _graphDbContext.WriteMapToStore(context);
//        }

//        return fileId;
//    }
//}
