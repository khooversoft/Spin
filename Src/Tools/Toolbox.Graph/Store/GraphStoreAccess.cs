using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphStoreAccess
{
    private readonly GraphDbContext _graphDbContext;
    internal GraphStoreAccess(GraphDbContext graphDbContext) => _graphDbContext = graphDbContext.NotNull();

    public Task<Option> Add(string fileId, string json, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public async Task<Option<string>> Add<T>(string nodeKey, string name, T value, ScopeContext context) where T : class
    {
        using var scope = await _graphDbContext.ReadWriterLock.WriterLockAsync();

        if (!_graphDbContext.IsNodeExist(nodeKey)) return (StatusCode.NotFound, $"NodeKey={nodeKey} not found");
        if (_graphDbContext.HasFileId(nodeKey, name)) return (StatusCode.Conflict, $"NodeKey={nodeKey} name={name} already exist");

        string fileId = CreateFileId(nodeKey, name);
        _graphDbContext.SetFileId(nodeKey, fileId);
        await _graphDbContext.Write(context);

        var addOption = await _graphDbContext.GraphStore.Add<T>(fileId, value, context);
        if (addOption.IsError()) return addOption.ToOptionStatus<string>();

        return fileId;
    }

    public async Task<Option> Delete(string nodeKey, string name, ScopeContext context)
    {
        using var scope = await _graphDbContext.ReadWriterLock.WriterLockAsync();

        string fileId = CreateFileId(nodeKey, name);
        _graphDbContext.RemoveFileId(nodeKey, fileId);
        await _graphDbContext.Write(context);

        var result = await _graphDbContext.GraphStore.Delete(fileId, context);
        return result;
    }

    public Task<Option> Exist(string nodeKey, string name, ScopeContext context)
    {
        string fileId = CreateFileId(nodeKey, name);
        return _graphDbContext.GraphStore.Exist(fileId, context);
    }

    public async Task<Option<T>> Get<T>(string nodeKey, string name, ScopeContext context)
    {
        string fileId = CreateFileId(nodeKey, name);
        var jsonOption = await _graphDbContext.GraphStore.Get<T>(fileId, context);
        if (jsonOption.IsError()) return jsonOption;

        return jsonOption.Return();
    }

    public async Task<Option> Set<T>(string nodeKey, string name, T value, ScopeContext context) where T : class
    {
        using var scope = await _graphDbContext.ReadWriterLock.WriterLockAsync();

        if (!_graphDbContext.IsNodeExist(nodeKey)) return (StatusCode.NotFound, $"NodeKey={nodeKey} not found");

        string fileId = CreateFileId(nodeKey, name);
        _graphDbContext.SetFileId(nodeKey, fileId);
        await _graphDbContext.Write(context);

        return await _graphDbContext.GraphStore.Set<T>(fileId, value, context);
    }

    public static string CreateFileId(string nodeKey, string name)
    {
        nodeKey.NotEmpty();
        name.Assert(x => IdPatterns.IsName(x), x => $"{x} invalid name");

        string file = nodeKey.NotEmpty().Replace('/', '_');

        string storePath = nodeKey
            .Split(new char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Func(x => x.Length > 1 ? x[0..(x.Length - 1)] : Array.Empty<string>())
            .Join('/');

        string result = storePath.IsEmpty() switch
        {
            true => $"nodes/{name}/{file}",
            false => $"nodes/{name}/{storePath}/{file}",
        };

        return result.ToLower();
    }
}
