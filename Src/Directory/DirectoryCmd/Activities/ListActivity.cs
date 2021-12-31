using Directory.sdk.Client;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace DirectoryCmd.Activities;

internal class ListActivity
{
    private readonly DirectoryClient _directoryClient;
    private readonly ILogger<ListActivity> _logger;

    public ListActivity(DirectoryClient directoryClient, ILogger<ListActivity> logger)
    {
        _directoryClient = directoryClient;
        _logger = logger;
    }

    public async Task List(string path, bool recursive, CancellationToken token)
    {
        path.VerifyNotEmpty(nameof(path));

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(List), Path = path });

        var query = new QueryParameter()
        {
            Filter = path,
            Recursive = recursive,
        };

        BatchSetCursor<DatalakePathItem> batch = _directoryClient.Search(query);
        int index = 0;

        var list = new List<string>
            {
                $"Listing directory from path {path}, recursive={recursive}...",
                "",
            };

        while (true)
        {
            BatchSet<DatalakePathItem> batchSet = await batch.ReadNext(token);
            if (batchSet.IsEndSignaled) break;

            batchSet.Records
                .Where(x => !(x.IsDirectory == true))
                .ForEach(x => list.Add($"({index++}) {x.Name}"));
        }

        list.Add("");
        list.Add($"Completed, {index} listed");

        _logger.LogInformation(list.Join(Environment.NewLine));
    }
}
