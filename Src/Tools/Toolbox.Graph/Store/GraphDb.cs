using Toolbox.Store;

namespace Toolbox.Graph;

public class GraphDb
{
    private readonly GraphDbAccess _dbContext;
    public GraphDb(IFileStore graphStore, IChangeTrace changeTrace) => _dbContext = new GraphDbAccess(graphStore, changeTrace);

    public GraphEntityAccess Entity => _dbContext.Entity;
    public GraphAccess Graph => _dbContext.Graph;
    public GraphStoreAccess Store => _dbContext.Store;
}
