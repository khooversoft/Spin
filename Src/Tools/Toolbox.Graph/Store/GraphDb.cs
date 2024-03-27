using Toolbox.Store;

namespace Toolbox.Graph;

public class GraphDb
{
    private readonly GraphDbContext _dbContext;

    public GraphDb(IFileStore graphStore)
    {
        _dbContext = new GraphDbContext(graphStore);
        Graph = new GraphAccess(_dbContext);
        Store = new GraphStoreAccess(_dbContext);
    }

    public GraphAccess Graph { get; }
    public GraphStoreAccess Store { get; }
}
