//using Microsoft.Extensions.Logging.Abstractions;
//using Toolbox.Store;
//using Toolbox.Tools;

//namespace Toolbox.Graph;

//public class GraphInMemory
//{
//    private readonly GraphMemoryContext _dbContext;

//    public GraphInMemory()
//        : this(new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance), new InMemoryChangeTrace())
//    {
//    }

//    public GraphInMemory(IFileStore fileStore, IChangeTrace changeTrace)
//    {
//        FileStore = fileStore.NotNull();
//        ChangeTrace = changeTrace.NotNull();

//        var shimmedFileStore = new FileStoreTraceShim(FileStore, ChangeTrace);
//        _dbContext = new GraphMemoryContext(shimmedFileStore, ChangeTrace);
//    }

//    public IGraphEntity Entity => _dbContext.Entity;
//    public IGraphCommand Graph => _dbContext.Command;
//    public IGraphStore Store => _dbContext.Store;

//    public IFileStore FileStore { get; }
//    public IChangeTrace ChangeTrace { get; }
//}
