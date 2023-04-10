using Directory.sdk.Client;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions;
using Toolbox.Abstractions.Extensions;
using Toolbox.Abstractions.Tools;
using Toolbox.Extensions;

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
        documentId.NotEmpty();

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(DeleteEntry), DirectoryId = documentId }).NotNull();

        var id = new DocumentId(documentId);
        await _directoryClient.Delete(id, token);

        _logger.LogInformation($"Deleted {documentId} from directory");
    }

    internal async Task DeleteProperty(string documentId, string[] properties, CancellationToken token)
    {
        documentId.NotEmpty();
        properties.NotNull();

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(DeleteEntry), DirectoryId = documentId }).NotNull();

        var id = new DocumentId(documentId);
        DirectoryEntry entry = (await _directoryClient.Get(id))
            .NotNull(name: $"{documentId} does not exist");

        var hash = new HashSet<string>(properties);

        var currentProperties = entry.Properties
            .Where(x => !hash.Contains(x));

        entry = entry with { Properties = currentProperties.ToList() };

        await _directoryClient.Set(entry, token);

        _logger.LogInformation($"Removed properties {properties.Join(", ")} from {documentId}");
    }
}
