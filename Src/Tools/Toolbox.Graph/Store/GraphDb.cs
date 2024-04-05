using Toolbox.Store;

namespace Toolbox.Graph;

public class GraphDb
{
    private readonly GraphDbContext _dbContext;

    public GraphDb(IFileStore graphStore) => _dbContext = new GraphDbContext(graphStore);

    public GraphEntityAccess Entity => _dbContext.Entity;
    public GraphAccess Graph => _dbContext.Graph;
    public GraphStoreAccess Store => _dbContext.Store;
}
