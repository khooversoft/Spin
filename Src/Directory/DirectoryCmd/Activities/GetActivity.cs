using Directory.sdk.Client;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace DirectoryCmd.Activities;

internal class GetActivity
{
    private readonly DirectoryClient _directoryClient;
    private readonly ILogger<GetActivity> _logger;

    public GetActivity(DirectoryClient directoryClient, ILogger<GetActivity> logger)
    {
        _directoryClient = directoryClient;
        _logger = logger;
    }

    public async Task WriteFile(string file, string path, bool recursive, CancellationToken token)
    {
        file.VerifyNotEmpty(nameof(file));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(WriteFile), File=file, Path=path, Recursive=recursive });

        _logger.LogInformation($"Reading directory at {path}, recursive={recursive}...");

        var query = new QueryParameter()
        {
            Filter = path,
            Recursive = recursive,
        };

        BatchSetCursor<DatalakePathItem> batch = _directoryClient.Search(query);

        var list = new List<DirectoryEntry>();

        while (true)
        {
            BatchSet<DatalakePathItem> batchSet = await batch.ReadNext(token);
            if (batchSet.IsEndSignaled) break;

            foreach(var entry in batchSet.Records)
            {
                var documentId = new DocumentId(entry.Name);

                DirectoryEntry? directoryEntry = await _directoryClient.Get(documentId, token);
                if( directoryEntry == null)
                {
                    _logger.LogWarning($"Directory entry for {entry.Name} was not found");
                    continue;
                }

                list.Add(directoryEntry);
            }
        }

        string json = Json.Default.SerializeFormat(list);
        File.WriteAllText(path, json);

        _logger.LogInformation($"{list.Count} directory entries written to {file}");
    }

    internal async Task DumpProperty(string directoryId, CancellationToken token)
    {
        var id = new DocumentId(directoryId);

        DirectoryEntry directoryEntry = (await _directoryClient.Get(id, token))
            .VerifyNotNull($"{directoryId} not found");

        string lines = new[]
        {
            $"Dumping {directoryId}...",
            "",
            Json.Default.SerializeFormat(directoryId),
        }.Join(Environment.NewLine);

        _logger.LogInformation(lines);
    }
}
