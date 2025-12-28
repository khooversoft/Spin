using System.Collections.Immutable;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public class DatalakeStore : IFileStore
{
    private readonly DataLakeFileSystemClient _fileSystem;
    private readonly ILogger<DatalakeStore> _logger;
    private readonly DatalakeOption _datalakeOption;
    private readonly DataLakeServiceClient _serviceClient;

    public DatalakeStore(DatalakeOption datalakeOption, ILogger<DatalakeStore> logger)
    {
        _datalakeOption = datalakeOption.NotNull().Action(x => x.Validate().ThrowOnError());
        _logger = logger.NotNull();

        _serviceClient = datalakeOption.CreateDataLakeServiceClient();

        _fileSystem = _serviceClient.GetFileSystemClient(datalakeOption.Container);
        _fileSystem.Exists().Assert(x => x == true, $"Datalake file system does not exist, containerName={datalakeOption.Container}");
    }

    public IFileAccess File(string path) => path
        .Func(x => _datalakeOption.WithBasePath(x))
        .Func(x => _fileSystem.GetFileClient(x))
        .Func(x => new DatalakeFileAccess(this, x, _logger));

    public async Task<Option> DeleteFolder(string path, ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-deleteDirectory", "path={path}", path);
        //DataChangeLog.GetRecorder().Assert(x => x == null, "DeleteFolder is not supported with DataChangeRecorder");

        path = _datalakeOption.WithBasePath(path);
        context.LogDebug("Deleting directory {path}", path);

        try
        {
            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            var response = await directoryClient.DeleteAsync(cancellationToken: context);
            if (response.Status != 200) return (StatusCode.Conflict, response.ReasonPhrase);

            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to delete directory for {path}", path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern, ScopeContext context)
    {
        context = context.With(_logger);
        var queryParameter = QueryParameter.Parse(pattern);
        using var metric = context.LogDuration("dataLakeStore-search", "queryParameter={queryParameter}", queryParameter);

        queryParameter = queryParameter with
        {
            Filter = _datalakeOption.WithBasePath(queryParameter.Filter),
            BasePath = _datalakeOption.WithBasePath(queryParameter.BasePath),
        };
        context.LogDebug("Searching {queryParameter}", queryParameter);

        var collection = new Sequence<PathItem>();
        var matcher = queryParameter.GetMatcher();

        int index = -1;
        try
        {
            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(queryParameter.BasePath, queryParameter.Recurse, cancellationToken: context))
            {
                if (!matcher.IsMatch(pathItem.Name, pathItem.IsDirectory == true)) continue;

                index++;
                if (index < queryParameter.Index) continue;

                collection += pathItem;
                if (collection.Count >= queryParameter.Count) break;
            }

            IReadOnlyList<StorePathDetail> list = collection
                .Select(x => x.ConvertTo(_datalakeOption.RemoveBaseRoot(x.Name)))
                .ToImmutableArray();

            return list;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
        {
            return Array.Empty<StorePathDetail>();
        }
        catch (Exception ex)
        {
            context.Location().LogWarning(ex, "Failed to search, query={queryParameter}", queryParameter);
            return Array.Empty<StorePathDetail>();
        }
    }
}
