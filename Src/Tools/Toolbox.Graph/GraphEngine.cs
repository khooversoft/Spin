using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphEngine
{
    GraphMapDataManager DataManager { get; }
    IDataClient<DataETag> DataClient { get; }
}

public class GraphEngine : IGraphEngine
{
    private readonly ILogger<GraphEngine> _logger;

    public GraphEngine(GraphMapDataManager dataManager, IDataClient<DataETag> dataClient, ILogger<GraphEngine> logger)
    {
        DataManager = dataManager.NotNull();
        DataClient = dataClient.NotNull();
        _logger = logger.NotNull();
    }

    public GraphMapDataManager DataManager { get; }
    public IDataClient<DataETag> DataClient { get; }
}
