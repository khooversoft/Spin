using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphStoreAccess
{
    private readonly GraphDbAccess _graphDbContext;
    private readonly IFileStore _graphStore;

    internal GraphStoreAccess(GraphDbAccess graphDbContext, IFileStore graphStore)
    {
        _graphDbContext = graphDbContext.NotNull();
        _graphStore = graphStore.NotNull();
    }

    public Task<Option<string>> Add<T>(string nodeKey, string name, T value, ScopeContext context) where T : class => AddOrUpdate(nodeKey, name, false, value, context);

    public async Task<Option> Delete(string nodeKey, string name, ScopeContext context)
    {
        if (!_graphDbContext.Map.Nodes.ContainsKey(nodeKey)) return (StatusCode.NotFound, $"NodeKey={nodeKey} not found");
        using var scope = await _graphDbContext.ReadWriterLock.WriterLockAsync();

        string fileId = GraphTool.CreateFileId(nodeKey, name);

        await _graphDbContext.Write(context);
        await _graphStore.Delete(fileId, context);

        return StatusCode.OK;
    }

    public Task<Option> Exist(string nodeKey, string name, ScopeContext context)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        return _graphStore.Exist(fileId, context);
    }

    public async Task<Option<T>> Get<T>(string nodeKey, string name, ScopeContext context)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        var jsonOption = await _graphStore.Get<T>(fileId, context);
        if (jsonOption.IsError()) return jsonOption;

        return jsonOption.Return();
    }

    public Task<Option<string>> Set<T>(string nodeKey, string name, T value, ScopeContext context) where T : class => AddOrUpdate(nodeKey, name, true, value, context);

    private async Task<Option<string>> AddOrUpdate<T>(string nodeKey, string name, bool upsert, T value, ScopeContext context) where T : class
    {
        if (!_graphDbContext.Map.Nodes.ContainsKey(nodeKey)) return (StatusCode.NotFound, $"NodeKey={nodeKey} not found");

        string fileId = GraphTool.CreateFileId(nodeKey, name);

        var addOption = upsert switch
        {
            false => await _graphStore.Add<T>(fileId, value, context),
            true => await _graphStore.Set<T>(fileId, value, context),
        };

        if (addOption.IsError()) return addOption.ToOptionStatus<string>();

        string cmd = $"update (key={nodeKey}) set link={fileId};";
        var updateResult = await _graphDbContext.Graph.ExecuteScalar(cmd, context);

        if (updateResult.StatusCode.IsError())
        {
            await _graphStore.Delete(fileId, context);
            return (StatusCode.Conflict, $"NodeKey={nodeKey} does not exist for update");
        }

        using var release = await _graphDbContext.Map.ReadWriterLock.ReaderLockAsync();
        await _graphDbContext.Write(context);
        return fileId;
    }
}
