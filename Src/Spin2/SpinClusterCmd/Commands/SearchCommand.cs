using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Client;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class SearchCommand : Command
{
    private readonly SpinClusterClient _client;
    private readonly ILogger<LeaseCommand> _logger;

    public SearchCommand(SpinClusterClient client, ILogger<LeaseCommand> logger)
        : base("search", "Search for files")
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        Argument<string> filterArgument = new Argument<string>("filter", "Filter to search for, schema is required, example: {schema}[/tenant[/path...]]");
        Argument<int?> indexArgument = new Argument<int?>("index", "Index (offset), default is 0");
        Argument<int?> countArgument = new Argument<int?>("count", "Number of items to be returned, default is 1000");
        System.CommandLine.Option<bool?> recurseArgument = new System.CommandLine.Option<bool?>("--recurse", "Recursive search");

        AddArgument(filterArgument);
        AddArgument(indexArgument);
        AddArgument(countArgument);
        AddOption(recurseArgument);

        this.SetHandler(async (filter, index, count, recurse) =>
        {
            var context = new ScopeContext(_logger);

            Toolbox.Types.Option<IReadOnlyList<StorePathItem>> files = await _client.Resource.Search(filter, index, count, recurse, context);

            context.Location().Log(files.IsOk() ? LogLevel.Information : LogLevel.Error, "Search filter={filter}, statusCode={statusCode}", filter, files.StatusCode);

            var logLine = new string?[]
            {
                "",
                "Search results...",
                "",
                files.IsError() ? $"Search failed, statusCode={files.StatusCode}, error={files.Error}" : null,
            }
            .Where(x => x != null)
            .Concat(files.IsOk() ?
                files.Return().Select(x => $"Name={x.Name}, IsDirectory={x.IsDirectory}, LastModified={x.LastModified}, ETag={x.ETag}, length={x.ContentLength}") :
                Enumerable.Empty<string>()
                )
            .Join(Environment.NewLine);

            _logger.LogInformation(logLine);


        }, filterArgument, indexArgument, countArgument, recurseArgument);
    }
}
