using Azure;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public class DatalakeStore : IDatalakeStore
{
    private readonly DataLakeFileSystemClient _fileSystem;
    private readonly DatalakeOption _azureStoreOption;
    private readonly ILogger<DatalakeStore> _logger;
    private readonly DataLakeServiceClient _serviceClient;

    public DatalakeStore(DatalakeOption azureStoreOption, ILogger<DatalakeStore> logger)
    {
        azureStoreOption.Validate().ThrowOnError();
        _azureStoreOption = azureStoreOption;
        _logger = logger.NotNull();

        _serviceClient = azureStoreOption.CreateDataLakeServiceClient();

        _fileSystem = _serviceClient.GetFileSystemClient(azureStoreOption.Container);
        _fileSystem.Exists().Assert(x => x == true, $"Datalake file system does not exist, containerName={azureStoreOption.Container}");
    }

    public async Task<Option> Append(string path, DataETag data, ScopeContext context)
    {
        context = context.With(_logger);
        data.NotNull().Assert(x => x.Data.Length > 0, $"{nameof(data)} length must be greater then 0, path={path}");
        using var metric = context.LogDuration("dataLakeStore-append", "path={path}, dataSize={dataSize}", path, data.Data.Length);

        path = _azureStoreOption.WithBasePath(path);
        context.Location().LogTrace("Appending to {path}, data.Length={data.Length}", path, data.Data.Length);

        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

        try
        {
            Option<DatalakePathProperties> propertiesOption = await GetPathPropertiesOrCreate(path, context).ConfigureAwait(false);
            if (propertiesOption.IsError()) return propertiesOption.ToOptionStatus();
            var properties = propertiesOption.Return();

            DataLakeFileClient file = _fileSystem.GetFileClient(path);

            await file.AppendAsync(memoryBuffer, properties.ContentLength, cancellationToken: context).ConfigureAwait(false);
            await file.FlushAsync(properties.ContentLength + data.Data.Length).ConfigureAwait(false);

            context.Location().LogTrace("Appended to path={path}", path);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
        {
            context.Location().LogTrace("Creating path={path}", path);
            await Write(path, data, true, context).ConfigureAwait(false);

            return StatusCode.OK;
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to append file {path}", path);
            return (StatusCode.BadRequest, $"Failed to append file {path}");
        }

        return StatusCode.OK;
    }

    public async Task<Option> Delete(string path, ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-delete", "path={path}", path);

        path = _azureStoreOption.WithBasePath(path);
        context.Location().LogTrace("Deleting to {path}", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.DeleteIfExistsAsync(cancellationToken: context).ConfigureAwait(false);

            if (!response.Value) context.Location().LogTrace("File path={path} does not exist", path);

            return response.Value ? StatusCode.OK : StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to delete file {path}", path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<Option> DeleteDirectory(string path, ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-deleteDirectory", "path={path}", path);

        path = _azureStoreOption.WithBasePath(path);
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

    public async Task<Option> Exist(string path, ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-exist", "path={path}", path);

        path = _azureStoreOption.WithBasePath(path);
        context.Location().LogTrace("Is path {path} exist", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.ExistsAsync(context).ConfigureAwait(false);
            return response.Value ? StatusCode.OK : StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to ExistsAsync for {path}", path);
            throw;
        }
    }

    public Task<Option<DatalakePathProperties>> GetPathProperties(string path, ScopeContext context)
    {
        context = context.With(_logger);
        path = _azureStoreOption.WithBasePath(path);

        context.Location().LogTrace("Getting path {path} properties", path);
        return InternalGetPathProperties(path, context);
    }

    public async Task<Option<DataETag>> Read(string path, ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-read", "path={path}", path);

        path = _azureStoreOption.WithBasePath(path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            var ifExists = await file.ExistsAsync(context).ConfigureAwait(false);
            if (ifExists.Value == false) return StatusCode.NotFound;
            metric.Log("existsAsync");

            Response<FileDownloadInfo> response = await file.ReadAsync(context).ConfigureAwait(false);
            if (response.Value == null) return StatusCode.NotFound;
            metric.Log("readAsync");

            using MemoryStream memory = new MemoryStream();
            await response.Value.Content.CopyToAsync(memory).ConfigureAwait(false);

            byte[] data = memory.ToArray();
            string etag = response.Value.Properties.ETag.ToString();
            context.Location().LogTrace("Read file {path}, size={size}, eTag={etag}", path, data.Length, etag);
            return new DataETag(data, etag);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            context.Location().LogTrace("File not found {path}", path);
            return (StatusCode.NotFound, $"File not found, path={path}");
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to read file {path}", path);
            return (StatusCode.BadRequest, ex.ToString());
        }
    }

    public async Task<Option<QueryResponse<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context)
    {
        context = context.With(_logger);
        queryParameter.NotNull();
        using var metric = context.LogDuration("dataLakeStore-search", "queryParameter={queryParameter}", queryParameter);

        queryParameter = queryParameter with
        {
            Filter = _azureStoreOption.WithBasePath(queryParameter.Filter),
            BasePath = _azureStoreOption.WithBasePath(queryParameter.BasePath),
        };
        context.Location().LogTrace("Searching {queryParameter}", queryParameter);

        var collection = new List<DatalakePathItem>();
        var matcher = queryParameter.GetMatcher();

        int index = -1;
        try
        {
            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(queryParameter.BasePath, queryParameter.Recurse, cancellationToken: context))
            {
                if (!matcher.IsMatch(pathItem.Name, pathItem.IsDirectory == true)) continue;

                index++;
                if (index < queryParameter.Index) continue;

                DatalakePathItem datalakePathItem = pathItem.ConvertTo();

                collection.Add(datalakePathItem);
                if (collection.Count >= queryParameter.Count) break;
            }

            var list = collection
                .Select(x => x with { Name = _azureStoreOption.RemoveBaseRoot(x.Name) })
                .ToList();

            return new QueryResponse<DatalakePathItem>
            {
                Query = queryParameter with { Index = index },
                Items = list,
                EndOfSearch = list.Count == 0,
            };
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
        {
            return new QueryResponse<DatalakePathItem>
            {
                Query = queryParameter,
                EndOfSearch = false,
            };
        }
        catch (Exception ex)
        {
            context.Location().LogWarning(ex, "Failed to search, query={queryParameter}", queryParameter);
            return (StatusCode.BadRequest, ex.ToString());
        }
    }

    public async Task<Option<ETag>> Write(string path, DataETag data, bool overwrite, ScopeContext context)
    {
        context = context.With(_logger);

        path = _azureStoreOption.WithBasePath(path);
        context.Location().LogTrace($"Writing to {path}, data.Length={data.Data.Length}, eTag={data.ETag?.ToString() ?? "<null>"}");

        data.NotNull().Assert(x => x.Data.Length > 0, $"length must be greater then 0, path={path}");

        context.Location().LogTrace("Writing path={path}", path);
        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

        return await Upload(path, memoryBuffer, overwrite, data, context).ConfigureAwait(false);
    }

    public async Task<Option> TestConnection(ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogTrace("Testing connection");

        try
        {
            Response<bool> response = await _fileSystem.ExistsAsync(cancellationToken: context).ConfigureAwait(false);
            context.Location().LogTrace("Testing exist of file system, exists={fileSystemExist}", response.Value);
            return response.Value ? StatusCode.OK : StatusCode.ServiceUnavailable;
        }
        catch (Exception ex)
        {
            context.Location().LogWarning(ex, "Failed exist for file systgem");
            return StatusCode.ServiceUnavailable;
        }
    }

    private async Task<Option<ETag>> Upload(string path, Stream fromStream, bool overwrite, DataETag dataETag, ScopeContext context)
    {
        Response<PathInfo> result;
        using var metric = context.LogDuration("dataLakeStore-upload", "path={path}", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);

            if (dataETag.ETag != default)
            {
                var option = new DataLakeFileUploadOptions
                {
                    Conditions = new DataLakeRequestConditions { IfMatch = dataETag.ETag.IsNotEmpty() ? new ETag(dataETag.ETag) : null }
                };

                result = await file.UploadAsync(fromStream, option, context).ConfigureAwait(false);
                return result.Value.ETag;
            }

            result = await file.UploadAsync(fromStream, overwrite, context).ConfigureAwait(false);
            return result.Value.ETag;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to upload {path}", path);
            return new Option<ETag>(StatusCode.InternalServerError);
        }
    }


    private async Task<Option<DatalakePathProperties>> GetPathPropertiesOrCreate(string path, ScopeContext context)
    {
        using var metric = context.LogDuration("dataLakeStore-getPathPropertiesOrCreate");

        var properties = await InternalGetPathProperties(path, context).ConfigureAwait(false);
        if (properties.IsOk()) return properties;

        await _fileSystem.GetFileClient(path).CreateIfNotExistsAsync(PathResourceType.File);
        return await InternalGetPathProperties(path, context).ConfigureAwait(false);
    }

    private async Task<Option<DatalakePathProperties>> InternalGetPathProperties(string path, ScopeContext context)
    {
        context.Location().LogTrace("Getting path {path} properties", path);
        using var metric = context.LogDuration("dataLakeStore-getPathProperties");

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> exist = await file.ExistsAsync();
            if (!exist.HasValue || !exist.Value)
            {
                context.Location().LogTrace("File does not exist, path={path}", path);
                return new Option<DatalakePathProperties>(StatusCode.NotFound);
            }

            var result = await file.GetPropertiesAsync(cancellationToken: context).ConfigureAwait(false);
            return result.Value.ConvertTo(path);
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to GetPathProperties for file {path}", path);
            return new Option<DatalakePathProperties>(StatusCode.NotFound);
        }
    }
}