﻿using Azure;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Azure.DataLake;

public class DatalakeStore : IDatalakeStore
{
    private readonly DataLakeFileSystemClient _fileSystem;
    private readonly DatalakeOption _azureStoreOption;
    private readonly ILogger<DatalakeStore> _logger;
    private readonly DataLakeServiceClient _serviceClient;

    public DatalakeStore(DatalakeOption azureStoreOption, ILogger<DatalakeStore> logger)
    {
        _azureStoreOption = azureStoreOption.Verify();
        _logger = logger.NotNull();

        _serviceClient = azureStoreOption.CreateDataLakeServiceClient();

        _fileSystem = _serviceClient.GetFileSystemClient(azureStoreOption.ContainerName);
    }

    public async Task<StatusCode> Append(string path, byte[] data, ScopeContext context)
    {
        path = WithBasePath(path);
        _logger.LogTrace(context.Location(), "Appending to {path}, data.Length={data.Length}", path, data.Length);

        data.NotNull().Assert(x => x.Length > 0, $"{nameof(data)} length must be greater then 0");

        using var memoryBuffer = new MemoryStream(data.ToArray());

        try
        {
            Option<DatalakePathProperties> properties = await InternalGetPathProperties(path, context);
            if (properties.IsError()) return StatusCode.NotFound;

            DataLakeFileClient file = _fileSystem.GetFileClient(path);

            await file.AppendAsync(memoryBuffer, properties.Return().ContentLength, cancellationToken: context);
            await file.FlushAsync(properties.Return().ContentLength + data.Length);

            _logger.LogInformation(context.Location(), "Appended to path={path}", path);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
        {
            _logger.LogInformation(context.Location(), "Creating path={path}", path);
            await Write(path, data, true, context);

            return StatusCode.OK;
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to append file {path}", path);
            return StatusCode.BadRequest;
        }

        return StatusCode.OK;
    }

    public async Task<StatusCode> Delete(string path, ScopeContext context)
    {
        path = WithBasePath(path);
        _logger.LogTrace(context.Location(), "Deleting to {path}", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.DeleteIfExistsAsync(cancellationToken: context);

            return response.Value ? StatusCode.OK : StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to delete file {path}", path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<StatusCode> DeleteDirectory(string path, ScopeContext context)
    {
        path = WithBasePath(path);
        _logger.LogTrace(context.Location(), "Deleting directory {path}", path);

        try
        {
            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            await directoryClient.DeleteAsync(cancellationToken: context);

            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to delete directory for {path}", path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<StatusCode> Exist(string path, ScopeContext context)
    {
        path = WithBasePath(path);
        _logger.LogTrace(context.Location(), "Is path {path} exist", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<bool> response = await file.ExistsAsync(context);
            return response.Value ? StatusCode.OK : StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to ExistsAsync for {path}", path);
            throw;
        }
    }

    public Task<Option<DatalakePathProperties>> GetPathProperties(string path, ScopeContext context)
    {
        path = WithBasePath(path);
        _logger.LogInformation(context.Location(), "Getting path {path} properties", path);

        return InternalGetPathProperties(path, context);
    }

    public async Task<Option<DataETag>> Read(string path, ScopeContext context)
    {
        path = WithBasePath(path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            Response<FileDownloadInfo> response = await file.ReadAsync(context);

            using MemoryStream memory = new MemoryStream();
            await response.Value.Content.CopyToAsync(memory);

            _logger.LogTrace(context.Location(), "Read file {path}", path);
            return new DataETag(memory.ToArray(), response.Value.Properties.ETag);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            _logger.LogError(context.Location(), ex, "Cannot read file {path}", path);
            return new Option<DataETag>(StatusCode.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to read file {path}", path);
            return new Option<DataETag>(StatusCode.BadRequest);
        }
    }

    public async Task<Option<IReadOnlyList<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context)
    {
        queryParameter.NotNull();
        queryParameter = queryParameter with { Filter = WithBasePath(queryParameter.Filter) };
        _logger.LogTrace(context.Location(), "Searching {queryParameter}", queryParameter);

        var list = new List<DatalakePathItem>();

        int index = -1;
        try
        {
            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(queryParameter.Filter, queryParameter.Recursive, cancellationToken: context))
            {
                index++;
                if (index < queryParameter.Index) continue;

                DatalakePathItem datalakePathItem = pathItem.ConvertTo();

                list.Add(datalakePathItem);
                if (list.Count >= queryParameter.Count) break;
            }

            return list
                .Select(x => x with { Name = RemoveBaseRoot(x.Name) })
                .ToList();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
        {
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to search, query={queryParameter}", queryParameter);
            return new Option<IReadOnlyList<DatalakePathItem>>(StatusCode.BadRequest);
        }
    }

    public async Task<Option<ETag>> Write(string path, DataETag data, bool overwrite, ScopeContext context)
    {
        path = WithBasePath(path);
        _logger.LogTrace(context.Location(), $"Writing to {path}, data.Length={data.Data.Length}, eTag={data.ETag?.ToString() ?? "<null>"}");

        data.NotNull().Assert(x => x.Data.Length > 0, "length must be greater then 0");

        _logger.LogTrace(context.Location(), "Writing path={path}", path);
        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

        return await Upload(path, memoryBuffer, overwrite, data, context);
    }

    private string WithBasePath(string? path) => _azureStoreOption.BasePath + (_azureStoreOption.BasePath.IsEmpty() ? string.Empty : "/") + path;

    private string RemoveBaseRoot(string path)
    {
        string newPath = path.Substring(_azureStoreOption.BasePath?.Length ?? 0);
        if (newPath.StartsWith("/")) newPath = newPath.Substring(1);

        return newPath;
    }

    private async Task<Option<ETag>> Upload(string path, Stream fromStream, bool overwrite, DataETag dataETag, ScopeContext context)
    {
        Response<PathInfo> result;

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);

            if (dataETag.ETag != null)
            {
                var option = new DataLakeFileUploadOptions
                {
                    Conditions = new DataLakeRequestConditions { IfMatch = dataETag.ETag }
                };

                result = await file.UploadAsync(fromStream, option, context);
                return result.Value.ETag;
            }

            result = await file.UploadAsync(fromStream, overwrite, context);
            return result.Value.ETag;
        }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to upload {path}", path);
            return new Option<ETag>(StatusCode.NotFound);
        }
    }

    private async Task<Option<DatalakePathProperties>> InternalGetPathProperties(string path, ScopeContext context)
    {
        _logger.LogTrace(context.Location(), "Getting path {path} properties", path);

        try
        {
            DataLakeFileClient file = _fileSystem.GetFileClient(path);
            var result = await file.GetPropertiesAsync(cancellationToken: context);

            return result.Value.ConvertTo(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(context.Location(), ex, "Failed to GetPathProperties for file {path}", path);
            return new Option<DatalakePathProperties>(StatusCode.BadRequest);
        }
    }
}