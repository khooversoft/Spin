using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphEngine
{
    GraphMapDataManager DataManager { get; }
    IKeyStore<DataETag> DataClient { get; }
    GraphLanguageParser LanguageParser { get; }
}

public class GraphEngine : IGraphEngine
{
    private readonly ILogger<GraphEngine> _logger;

    public GraphEngine(GraphMapDataManager dataManager, IKeyStore<DataETag> dataClient, GraphLanguageParser languageParser, ILogger<GraphEngine> logger)
    {
        DataManager = dataManager.NotNull();
        DataClient = dataClient.NotNull();
        _logger = logger.NotNull();
        LanguageParser = languageParser;
    }

    public GraphMapDataManager DataManager { get; }
    public IKeyStore<DataETag> DataClient { get; }
    public GraphLanguageParser LanguageParser { get; }
}
