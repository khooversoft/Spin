using Toolbox.Store;

namespace Toolbox.Graph;

public class GraphDbMemory
{
    private readonly GraphMemoryContext _dbContext;

    public GraphDbMemory(IFileStore fileStore, IChangeTrace changeTrace)
    {
        var shimmedFileStore = new FileStoreTraceShim(fileStore, changeTrace);
        _dbContext = new GraphMemoryContext(shimmedFileStore, changeTrace);
    }

    public IGraphEntity Entity => _dbContext.Entity;
    public IGraphCommand Graph => _dbContext.Graph;
    public IGraphStore Store => _dbContext.Store;
}
