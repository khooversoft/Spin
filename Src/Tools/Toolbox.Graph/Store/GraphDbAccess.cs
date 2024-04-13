using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class GraphDbAccess
{
    private const string _graphFileId = "directory.json";

    public GraphDbAccess(IFileStore store, IChangeTrace changeTrace)
    {
        FileStore = store.NotNull();
        ChangeTrace = changeTrace.NotNull();

        Graph = new GraphAccess(this);
        Store = new GraphStoreAccess(this, store);
        Entity = new GraphEntityAccess(this);
    }

    public GraphMap Map { get; private set; } = new GraphMap();
    public IFileStore FileStore { get; }
    public IChangeTrace ChangeTrace { get; }
    public GraphAccess Graph { get; }
    public GraphStoreAccess Store { get; }
    public GraphEntityAccess Entity { get; }
    public AsyncReaderWriterLock ReadWriterLock => Map.ReadWriterLock;

    public async Task<Option> ReadMapFromStore(ScopeContext context)
    {
        var gsOption = await FileStore.Get<GraphSerialization>(_graphFileId, context);
        if (gsOption.IsError()) return gsOption.ToOptionStatus();

        Map = gsOption.Return().FromSerialization();
        return StatusCode.OK;
    }

    public async Task<Option> WriteMapToStore(ScopeContext context)
    {
        var gs = Map.ToSerialization();
        return await FileStore.Set(_graphFileId, gs, context);
    }
}

