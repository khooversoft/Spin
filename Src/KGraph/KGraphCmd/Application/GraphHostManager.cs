using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Graph;
using Toolbox.Store;
using Toolbox.Tools;

namespace KGraphCmd.Application;

public class GraphHostManager : IDisposable
{
    private readonly ILogger<GraphHostManager> _logger;
    private GraphHostService? _graphHost;
    private string? _loadedJsonFile;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public GraphHostManager(ILogger<GraphHostManager> logger) => _logger = logger.NotNull();

    public IServiceProvider ServiceProvider => _graphHost.NotNull("Host not started").Services;

    public string? DumpFolder { get; private set; }

    public async Task<GraphHostService> Start(string jsonFile)
    {
        jsonFile.NotEmpty("Json file is required").Assert(x => File.Exists(x), x => $"File {x} does not exist");

        await _semaphore.WaitAsync();

        try
        {
            if (_graphHost != null && _loadedJsonFile == jsonFile) return _graphHost;

            Close();
            _loadedJsonFile = jsonFile;

            if (_graphHost != null) return _graphHost;

            _graphHost = await new GraphHostBuilder()
                .UseLogging()
                .SetConfigurationFile(_loadedJsonFile)
                .AddDatalakeFileStore()
                .Build();

            // See if we can connect to the datalake
            _graphHost.Services.GetRequiredService<IFileStore>();

            DumpFolder = _graphHost.Services.GetRequiredService<IConfiguration>()["DumpFolder"];
            return _graphHost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start graph host");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Close() => Interlocked.Exchange(ref _graphHost, null)?.Dispose();

    public void Dispose() => Close();
}
