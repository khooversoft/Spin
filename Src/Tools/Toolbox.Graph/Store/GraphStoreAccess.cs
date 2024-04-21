using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphStoreAccess
{
    private readonly GraphDbAccess _graphDbContext;
    private readonly IFileStore _graphFileStore;

    internal GraphStoreAccess(GraphDbAccess graphDbContext, IFileStore graphFileStore)
    {
        _graphDbContext = graphDbContext.NotNull();
        _graphFileStore = graphFileStore.NotNull();
    }

    public Task<Option<string>> Add<T>(string nodeKey, string name, T value, ScopeContext context) where T : class => AddOrUpdate(nodeKey, name, false, value, context);

    public async Task<Option> Delete(string nodeKey, string name, ScopeContext context)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);

        string cmd = $"update (key={nodeKey}) set link=-{fileId};";
        var updateResult = await _graphDbContext.Graph.ExecuteScalar(cmd, context);
        if (updateResult.StatusCode.IsError()) return updateResult.ToOptionStatus();
        if (updateResult.Return().Items.Count == 0) return StatusCode.NotFound;

        using (var scope = await _graphDbContext.ReadWriterLock.ReaderLockAsync())
        {
            await _graphDbContext.WriteMapToStore(context);
        }

        return await _graphFileStore.Delete(fileId, context);
    }

    public Task<Option> Exist(string nodeKey, string name, ScopeContext context)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        return _graphFileStore.Exist(fileId, context);
    }

    public async Task<Option<T>> Get<T>(string nodeKey, string name, ScopeContext context)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        var jsonOption = await _graphFileStore.Get<T>(fileId, context);
        if (jsonOption.IsError()) return jsonOption.ToOptionStatus<T>();

        return jsonOption.Return().Value;
    }

    public Task<Option<string>> Set<T>(string nodeKey, string name, T value, ScopeContext context) where T : class => AddOrUpdate(nodeKey, name, true, value, context);

    private async Task<Option<string>> AddOrUpdate<T>(string nodeKey, string name, bool upsert, T value, ScopeContext context) where T : class
    {
        if (!_graphDbContext.Map.Nodes.ContainsKey(nodeKey)) return (StatusCode.NotFound, $"NodeKey={nodeKey} not found");

        string fileId = GraphTool.CreateFileId(nodeKey, name);

        var addOption = upsert switch
        {
            false => await _graphFileStore.Add<T>(fileId, value, context),
            true => await _graphFileStore.Set<T>(fileId, value, context),
        };

        if (addOption.IsError()) return addOption;

        string cmd = $"update (key={nodeKey}) set link={fileId};";
        var updateResult = await _graphDbContext.Graph.ExecuteScalar(cmd, context);
        if (updateResult.StatusCode.IsError() || updateResult.Return().Items.Count == 0)
        {
            await _graphFileStore.Delete(fileId, context);
            return (StatusCode.Conflict, $"NodeKey={nodeKey} does not exist for update");
        }

        using (var release = await _graphDbContext.Map.ReadWriterLock.ReaderLockAsync())
        {
            await _graphDbContext.WriteMapToStore(context);
        }

        return fileId;
    }
}
