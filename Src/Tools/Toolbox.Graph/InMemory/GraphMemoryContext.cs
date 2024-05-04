//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//internal class GraphMemoryContext
//{
//    private const string _graphFileId = "directory.json";

//    public GraphMemoryContext(IFileStore store, IChangeTrace changeTrace)
//    {
//        FileStore = store.NotNull();
//        ChangeTrace = changeTrace.NotNull();

//        Command = new GraphCommandMemory(this);
//        Store = new GraphStoreMemory(this, store);
//        Entity = new GraphEntity(Command, Store);
//    }

//    public GraphMap Map { get; private set; } = new GraphMap();
//    public IFileStore FileStore { get; }
//    public IChangeTrace ChangeTrace { get; }
//    public IGraphCommand Command { get; }
//    public IGraphStore Store { get; }
//    public IGraphEntity Entity { get; }
//    public AsyncReaderWriterLock ReadWriterLock => Map.ReadWriterLock;

//    public async Task<Option> ReadMapFromStore(ScopeContext context)
//    {
//        var gsOption = await FileStore.Get<GraphSerialization>(_graphFileId, context);
//        if (gsOption.IsError()) return gsOption.ToOptionStatus();

//        Map = gsOption.Return().Value.FromSerialization();
//        return StatusCode.OK;
//    }

//    public async Task<Option> WriteMapToStore(ScopeContext context)
//    {
//        var gs = Map.ToSerialization();
//        var result = await FileStore.Set(_graphFileId, gs, context);
//        return result.ToOptionStatus();
//    }
//}

