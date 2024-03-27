using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class GraphDbContext
{
    private const string _graphFileId = "directory.json";

    public GraphDbContext(IFileStore store) => GraphStore = store.NotNull();

    public GraphMap Map { get; private set; } = new GraphMap();
    public IFileStore GraphStore { get; }
    public AsyncReaderWriterLock ReadWriterLock => Map.ReadWriterLock;

    public async Task<Option> Read(ScopeContext context)
    {
        var gsOption = await GraphStore.Get<GraphSerialization>(_graphFileId, context);
        if (gsOption.IsError()) return gsOption.ToOptionStatus();

        Map = gsOption.Return().FromSerialization();
        return StatusCode.OK;
    }

    public async Task<Option> Write(ScopeContext context)
    {
        var gs = Map.ToSerialization();
        return await GraphStore.Set(_graphFileId, gs, context);
    }

    public bool IsNodeExist(string nodeKey) => Map.Nodes.ContainsKey(nodeKey);

    public bool HasFileId(string nodeKey, string fileId) => Map.Nodes.TryGetValue(nodeKey, out var value) && value.FileIds.Contains(fileId, StringComparer.OrdinalIgnoreCase);

    public Option SetFileId(string nodeKey, string fileId) => IsNodeExist(nodeKey) switch
    {
        false => StatusCode.NotFound,
        true => HasFileId(nodeKey, fileId) switch
        {
            true => StatusCode.OK,
            false => StatusCode.OK.Action(x => Map.Nodes[nodeKey] = Map.Nodes[nodeKey].AddFileId(fileId)),
        },
    };

    public void RemoveFileId(string nodeKey, string fileId)
    {
        if (Map.Nodes.TryGetValue(nodeKey, out var value)) value.RemoveFileId(fileId);
    }
}
