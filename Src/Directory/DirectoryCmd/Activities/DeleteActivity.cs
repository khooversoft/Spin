using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Document;
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

    internal async Task DeleteEntry(string documentId, CancellationToken token)
    {
        documentId.VerifyNotEmpty(nameof(documentId));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(DeleteEntry), DirectoryId = documentId });

        var id = new DocumentId(documentId);
        await _directoryClient.Delete(id, token);

        _logger.LogInformation($"Deleted {documentId} from directory");
    }

    internal async Task DeleteProperty(string documentId, string[] properties, CancellationToken token)
    {
        documentId.VerifyNotEmpty(nameof(documentId));
        properties.VerifyNotNull(nameof(properties));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(DeleteEntry), DirectoryId = documentId });

        var id = new DocumentId(documentId);
        DirectoryEntry entry = (await _directoryClient.Get(id))
            .VerifyNotNull($"{documentId} does not exist");

        var hash = new HashSet<string>(properties);

        var currentProperties = entry.Properties
            .Where(x => !hash.Contains(x));

        entry = entry with { Properties = currentProperties.ToList() };

        await _directoryClient.Set(entry, token);

        _logger.LogInformation($"Removed properties {properties.Join(", ")} from {documentId}");
    }
}
