using Toolbox.Store;

namespace Toolbox.Graph;

public class GraphDb
{
    private readonly GraphDbAccess _dbContext;

    public GraphDb(IFileStore fileStore, IChangeTrace changeTrace)
    {
        var shimmedFileStore = new FileStoreTraceShim(fileStore, changeTrace);
        _dbContext = new GraphDbAccess(shimmedFileStore, changeTrace);
    }

    public GraphEntityAccess Entity => _dbContext.Entity;
    public GraphAccess Graph => _dbContext.Graph;
    public GraphStoreAccess Store => _dbContext.Store;
}
