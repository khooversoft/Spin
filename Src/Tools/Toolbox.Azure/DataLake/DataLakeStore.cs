using Azure;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
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

    public async Task<StatusCode> Append(string path, byte[] data, ScopeContext context)
    {
        context = context.With(_logger);

        path = WithBasePath(path);
        context.Location().LogTrace("Appending to {path}, data.Length={data.Length}", path, data.Length);

        data.NotNull().Assert(x => x.Length > 0, $"{nameof(data)} length must be greater then 0, path={path}");

        using var memoryBuffer = new MemoryStream(data.ToArray());

        try
        {
            Option<DatalakePathProperties> properties = await InternalGetPathProperties(path, context);
            if (properties.IsError()) return StatusCode.NotFound;

            DataLakeFileClient file = _fileSystem.GetFileClient(path);

            await file.AppendAsync(memoryBuffer, properties.Return().ContentLength, cancellationToken: context);
            await file.FlushAsync(properties.Return().ContentLength + data.Length);

            context.Location().LogInformation("Appended to path={path}", path);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
        {
            context.Location().LogInformation("Creating path={path}", path);
            await Write(path, data, true, context);

            return StatusCode.OK;
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to append file {path}", path);
            return StatusCode.BadRequest;
        }

        return StatusCode.OK;
    }

    public async Task<StatusCode> Delete(string path, ScopeContext context)
    {
        context = context.With(_logger);

        path = WithBasePath(path);
        context.Location().LogTrace("Deleting to {path}", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.DeleteIfExistsAsync(cancellationToken: context);

            if (!response.Value) context.Location().LogInformation("File path={path} does not exist", path);

            return response.Value ? StatusCode.OK : StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to delete file {path}", path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<StatusCode> DeleteDirectory(string path, ScopeContext context)
    {
        context = context.With(_logger);

        path = WithBasePath(path);
        context.Location().LogTrace("Deleting directory {path}", path);

        try
        {
            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            await directoryClient.DeleteAsync(cancellationToken: context);

            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to delete directory for {path}", path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<StatusCode> Exist(string path, ScopeContext context)
    {
        context = context.With(_logger);

        path = WithBasePath(path);
        context.Location().LogTrace("Is path {path} exist", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.ExistsAsync(context);
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

        path = WithBasePath(path);

        context.Location().LogInformation("Getting path {path} properties", path);
        return InternalGetPathProperties(path, context);
    }

    public async Task<Option<DataETag>> Read(string path, ScopeContext context)
    {
        context = context.With(_logger);

        path = WithBasePath(path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<FileDownloadInfo> response = await file.ReadAsync(context);

            using MemoryStream memory = new MemoryStream();
            await response.Value.Content.CopyToAsync(memory);

            byte[] data = memory.ToArray();
            string etag = response.Value.Properties.ETag.ToString();
            context.Location().LogInformation("Read file {path}, size={size}, eTag={etag}", path, data.Length, etag);
            return new DataETag(data, etag);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            context.Location().LogInformation("File not found {path}", path);
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
        queryParameter = queryParameter with { Filter = WithBasePath(queryParameter.Filter) };
        context.Location().LogTrace("Searching {queryParameter}", queryParameter);

        var collection = new List<DatalakePathItem>();

        string basePath = queryParameter.Filter
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .TakeWhile(x => x.IndexOf('*') < 0)
            .Join('/');

        bool hasWildcard = queryParameter.Filter != basePath;

        int index = -1;
        try
        {
            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(basePath, queryParameter.Recurse, cancellationToken: context))
            {
                if (hasWildcard && !pathItem.Name.Match(queryParameter.Filter)) continue;

                index++;
                if (index < queryParameter.Index) continue;

                DatalakePathItem datalakePathItem = pathItem.ConvertTo();

                collection.Add(datalakePathItem);
                if (collection.Count >= queryParameter.Count) break;
            }

            var list = collection
                .Select(x => x with { Name = RemoveBaseRoot(x.Name) })
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
            return new Option<QueryResponse<DatalakePathItem>>(StatusCode.BadRequest, ex.ToString());
        }
    }

    public async Task<Option<ETag>> Write(string path, DataETag data, bool overwrite, ScopeContext context)
    {
        context = context.With(_logger);

        path = WithBasePath(path);
        context.Location().LogTrace($"Writing to {path}, data.Length={data.Data.Length}, eTag={data.ETag?.ToString() ?? "<null>"}");

        data.NotNull().Assert(x => x.Data.Length > 0, $"length must be greater then 0, path={path}");

        context.Location().LogTrace("Writing path={path}", path);
        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

        return await Upload(path, memoryBuffer, overwrite, data, context);
    }

    public async Task<bool> TestConnection(ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogTrace("Testing connection");

        try
        {
            Response<bool> response = await _fileSystem.ExistsAsync(cancellationToken: context);
            context.Location().LogInformation("Testing exist of file system, exists={fileSystemExist}", response.Value);
            return response.Value;
        }
        catch (Exception ex)
        {
            context.Location().LogWarning(ex, "Failed exist for file systgem");
            return false;
        }
    }

    private string WithBasePath(string? path) => new[]
    {
        _azureStoreOption.BasePath,
        path,
    }.Where(x => x.IsNotEmpty())
    .Join('/')
    .RemoveTrailing('/');

    private string RemoveBaseRoot(string path)
    {
        string newPath = path[(_azureStoreOption.BasePath?.Length ?? 0)..];
        if (newPath.StartsWith("/")) newPath = newPath[1..];

        return newPath;
    }

    private async Task<Option<ETag>> Upload(string path, Stream fromStream, bool overwrite, DataETag dataETag, ScopeContext context)
    {
        Response<PathInfo> result;

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);

            if (dataETag.ETag != default)
            {
                var option = new DataLakeFileUploadOptions
                {
                    Conditions = new DataLakeRequestConditions { IfMatch = dataETag.ETag.IsNotEmpty() ? new ETag(dataETag.ETag) : null }
                };

                result = await file.UploadAsync(fromStream, option, context);
                return result.Value.ETag;
            }

            result = await file.UploadAsync(fromStream, overwrite, context);
            return result.Value.ETag;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to upload {path}", path);
            return new Option<ETag>(StatusCode.InternalServerError);
        }
    }


    private async Task<Option<DatalakePathProperties>> InternalGetPathProperties(string path, ScopeContext context)
    {
        context.Location().LogTrace("Getting path {path} properties", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            var result = await file.GetPropertiesAsync(cancellationToken: context);

            return result.Value.ConvertTo(path);
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to GetPathProperties for file {path}", path);
            return new Option<DatalakePathProperties>(StatusCode.BadRequest);
        }
    }
}