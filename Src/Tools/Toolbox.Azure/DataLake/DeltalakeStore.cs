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

public partial class DatalakeStore : IKeyStore
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

    public async Task<Option> DeleteFolder(string path)
    {
        using var metric = _logger.LogDuration("dataLakeStore-deleteDirectory", "path={path}", path);

        path = _datalakeOption.WithBasePath(path);
        _logger.LogDebug("Deleting directory {path}", path);

        try
        {
            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            var response = await directoryClient.DeleteAsync(recursive: true);
            if (response.Status != 200) return (StatusCode.Conflict, response.ReasonPhrase);

            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.Status == 401 || ex.ErrorCode == "PathNotFound")
        {
            _logger.LogError(ex, "Failed to delete directory for {path}", path);
            return StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete directory for {path}", path);
        }

        return StatusCode.BadRequest;
    }

    public async Task<Option<StorePathDetail>> GetDetails(string path)
    {
        var fileClient = GetFileClient(path);

        _logger.LogDebug("Getting path {path} properties", fileClient.Path);
        using var metric = _logger.LogDuration("dataLakeStore-getPathProperties");

        try
        {
            Response<bool> exist = await fileClient.ExistsAsync();
            if (!exist.HasValue || !exist.Value)
            {
                _logger.LogDebug("File does not exist, path={path}", fileClient.Path);
                return new Option<StorePathDetail>(StatusCode.NotFound);
            }

            var result = await fileClient.GetPropertiesAsync();
            return result.Value.ConvertTo(fileClient.Path).ToOption();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to GetPathProperties for file {path}", fileClient.Path);
            return (StatusCode.NotFound, ex.Message);
        }
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern)
    {
        var queryParameter = QueryParameter.Parse(pattern);
        using var metric = _logger.LogDuration("dataLakeStore-search", "queryParameter={queryParameter}", queryParameter);

        queryParameter = queryParameter with
        {
            Filter = _datalakeOption.WithBasePath(queryParameter.Filter),
            BasePath = _datalakeOption.WithBasePath(queryParameter.BasePath),
        };
        _logger.LogDebug("Searching {queryParameter}", queryParameter);

        var collection = new Sequence<PathItem>();
        var matcher = queryParameter.GetMatcher();

        int index = -1;
        try
        {
            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(queryParameter.BasePath, queryParameter.Recurse))
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
            _logger.LogWarning(ex, "Failed to search, query={queryParameter}", queryParameter);
            return Array.Empty<StorePathDetail>();
        }
    }

    private DataLakeFileClient GetFileClient(string path)
    {
        path.NotEmpty();

        var p1 = _datalakeOption.WithBasePath(path);
        var fullPath = _fileSystem.GetFileClient(p1);
        return fullPath;
    }
}
