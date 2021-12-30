using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DirectoryCmd.Activities;

internal class DeleteActivity
{
    private readonly DirectoryClient _directoryClient;
    private readonly ILogger<DeleteActivity> _logger;

    public DeleteActivity(DirectoryClient directoryClient, ILogger<DeleteActivity> logger)
    {
        _directoryClient = directoryClient;
        _logger = logger;
    }

    internal async Task DeleteEntry(string directoryId, CancellationToken token)
    {
        directoryId.VerifyNotEmpty(nameof(directoryId));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(DeleteEntry), DirectoryId = directoryId });

        var id = new DirectoryId(directoryId);
        await _directoryClient.Delete(id, token);

        _logger.LogInformation($"Deleted {directoryId} from directory");
    }

    internal async Task DeleteProperty(string directoryId, string[] properties, CancellationToken token)
    {
        directoryId.VerifyNotEmpty(nameof(directoryId));
        properties.VerifyNotNull(nameof(properties));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(DeleteEntry), DirectoryId = directoryId });

        var id = new DirectoryId(directoryId);
        DirectoryEntry entry = (await _directoryClient.Get(id))
            .VerifyNotNull($"{directoryId} does not exist");

        foreach (var key in properties)
        {
            entry.Properties.Remove(key);
        }

        await _directoryClient.Set(entry, token);

        _logger.LogInformation($"Removed properties {properties.Join(", ")} from {directoryId}");
    }
}
