using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Graph;

public interface IGraphEngine
{
    GraphMapStore GraphMapStore { get; }
    IKeyStore DataClient { get; }
    GraphLanguageParser LanguageParser { get; }
}

public class GraphEngine : IGraphEngine
{
    private readonly ILogger<GraphEngine> _logger;

    public GraphEngine(
        GraphMapStore graphMapStore,
        [FromKeyedServices(GraphConstants.File.Key)] IKeyStore dataFileClient,
        GraphLanguageParser languageParser,
        ILogger<GraphEngine> logger)
    {
        GraphMapStore = graphMapStore.NotNull();
        DataClient = dataFileClient.NotNull();
        LanguageParser = languageParser.NotNull();
        _logger = logger.NotNull();
    }

    public GraphMapStore GraphMapStore { get; }
    public IKeyStore DataClient { get; }
    public GraphLanguageParser LanguageParser { get; }
}
