using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphEngine
{
    GraphMapManager DataManager { get; }
    IKeyStore<DataETag> DataClient { get; }
    GraphLanguageParser LanguageParser { get; }
}

public class GraphEngine : IGraphEngine
{
    private readonly ILogger<GraphEngine> _logger;

    public GraphEngine(GraphMapManager dataManager, IKeyStore<DataETag> dataClient, GraphLanguageParser languageParser, ILogger<GraphEngine> logger)
    {
        DataManager = dataManager.NotNull();
        DataClient = dataClient.NotNull();
        _logger = logger.NotNull();
        LanguageParser = languageParser;
    }

    public GraphMapManager DataManager { get; }
    public IKeyStore<DataETag> DataClient { get; }
    public GraphLanguageParser LanguageParser { get; }

    public string StoreName => DataManager.StoreName;
    public Task<string> GetSnapshot() => DataManager.GetSnapshot();
    public string? GetLogSequenceNumber() => DataManager.GetLogSequenceNumber();
    public Task<Option> Recovery(IEnumerable<DataChangeRecord> records) => DataManager.Recovery(records);
    public void SetLogSequenceNumber(string lsn) => DataManager.SetLogSequenceNumber(lsn);
    public Task<Option> Checkpoint() => DataManager.Checkpoint();
    public Task<Option> Restore(string json) => DataManager.Restore(json);
}
