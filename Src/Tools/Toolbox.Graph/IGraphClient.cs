using Toolbox.Tools;

namespace Toolbox.Graph;

public interface IGraphClient
{
    IGraphCommand Command { get; }
    IGraphEntity Entity { get; }
    IGraphStore GraphStore { get; }
}

public class GraphClient : IGraphClient
{
    public GraphClient(IGraphCommand command, IGraphEntity entity, IGraphStore graphStore)
    {
        Command = command.NotNull();
        Entity = entity.NotNull();
        GraphStore = graphStore.NotNull();
    }

    public IGraphCommand Command { get; }

    public IGraphEntity Entity { get; }

    public IGraphStore GraphStore { get; }
}