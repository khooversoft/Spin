using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class GraphDbContext
{
    private const string _graphFileId = "directory.json";

    public GraphDbContext(IFileStore store)
    {
        GraphStore = store.NotNull();
        Graph = new GraphAccess(this);
        Store = new GraphStoreAccess(this, store);
        Entity = new GraphEntityAccess(this);
    }

    public GraphMap Map { get; private set; } = new GraphMap();
    public IFileStore GraphStore { get; }
    public GraphAccess Graph { get; }
    public GraphStoreAccess Store { get; }
    public GraphEntityAccess Entity { get; }
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
}
