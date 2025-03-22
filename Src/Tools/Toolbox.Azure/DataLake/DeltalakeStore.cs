using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Logging;
using System.Collections.Immutable;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Files.DataLake;
using Azure;

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
        .Func(x => new DatalakeFileAccess(x, _logger));

    public async Task<Option> DeleteFolder(string path, ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-deleteDirectory", "path={path}", path);

        path = _datalakeOption.WithBasePath(path);
        context.Location().LogTrace("Deleting directory {path}", path);

        try
        {
            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            var response = await directoryClient.DeleteAsync(cancellationToken: context).ConfigureAwait(false);
            if (response.Status != 200) return (StatusCode.Conflict, response.ReasonPhrase);

            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to delete directory for {path}", path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context)
    {
        var query = QueryParameter.Parse(pattern);
        var result = await Search(query, context).ConfigureAwait(false);
        return result;
    }

    private async Task<IReadOnlyList<IStorePathDetail>> Search(QueryParameter queryParameter, ScopeContext context)
    {
        context = context.With(_logger);
        queryParameter.NotNull();
        using var metric = context.LogDuration("dataLakeStore-search", "queryParameter={queryParameter}", queryParameter);

        queryParameter = queryParameter with
        {
            Filter = _datalakeOption.WithBasePath(queryParameter.Filter),
            BasePath = _datalakeOption.WithBasePath(queryParameter.BasePath),
        };
        context.Location().LogTrace("Searching {queryParameter}", queryParameter);

        var collection = new List<PathItem>();
        var matcher = queryParameter.GetMatcher();

        int index = -1;
        try
        {
            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(queryParameter.BasePath, queryParameter.Recurse, cancellationToken: context))
            {
                if (!matcher.IsMatch(pathItem.Name, pathItem.IsDirectory == true)) continue;

                index++;
                if (index < queryParameter.Index) continue;

                collection.Add(pathItem);
                if (collection.Count >= queryParameter.Count) break;
            }

            IReadOnlyList<IStorePathDetail> list = collection
                .Select(x => x.ConvertTo(_datalakeOption.RemoveBaseRoot(x.Name)))
                .ToImmutableArray();

            return list;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
        {
            return Array.Empty<IStorePathDetail>();
        }
        catch (Exception ex)
        {
            context.Location().LogWarning(ex, "Failed to search, query={queryParameter}", queryParameter);
            return Array.Empty<IStorePathDetail>();
        }
    }
}
