using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Graph;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Application;

public class GraphHostManager : IAsyncDisposable
{
    private readonly ILogger<GraphHostManager> _logger;
    private readonly object _lock = new object();
    private ServiceProvider? _serviceProvider;

    public GraphHostManager(ILogger<GraphHostManager> logger) => _logger = logger.NotNull();

    public ServiceProvider ServiceProvider => _serviceProvider.NotNull();

    public async ValueTask DisposeAsync() => await Close();

    public async ValueTask Close()
    {
        if (_serviceProvider == null) return;
        await _serviceProvider.DisposeAsync();
        _serviceProvider = null;
    }

    public IServiceProvider Start(string jsonFile)
    {
        lock (_lock)
        {
            _serviceProvider?.Dispose();
            _serviceProvider = HostTool.StartHost(jsonFile);

            try
            {
                IFileStore store = _serviceProvider.GetRequiredService<IFileStore>();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to connect to datalake");
                throw;
            }

            return _serviceProvider;
        }
    }

    public async Task LoadMap(ScopeContext context)
    {
        var host = _serviceProvider.NotNull().GetRequiredService<IGraphEngine>();
        context.LogInformation("Loading map...");
        var result = await host.InitializeDatabase(context);
        result.LogStatus(context, "Load map result");
    }
}
