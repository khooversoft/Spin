using System.Collections.Frozen;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public partial class DatalakeStore
{
    private static readonly FrozenDictionary<string, (StatusCode, string)> _statusErrors = new[]
    {
        ("LeaseIdMissing", (StatusCode.Locked, "No lease ID")),
        ("LeaseIdMismatch", (StatusCode.Locked, "Lease ID invalid")),
        ("LeaseNotPresent", (StatusCode.Locked, "Lease Not Present")),
        ("ConditionNotMet", (StatusCode.Conflict, "ETag is not valid (ConditionNotMet)")),
        ("PathAlreadyExists", (StatusCode.Conflict, "Path already exists")),
        ("PathNotFound", (StatusCode.Conflict, "Path not found")),
    }.Select(x => KeyValuePair.Create(x.Item1, x.Item2))
    .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public async Task<Option<string>> Add(string key, DataETag data)
    {
        var fileClient = GetFileClient(key);
        var result = await Upload(fileClient, false, data, null);

        //if (result.IsOk()) _recorder?.Add(key, data);
        return result;
    }

    public async Task<Option> Delete(string path, string? leaseId = null)
    {
        path.NotEmpty();
        using var metric = _logger.LogDuration("dataLakeStore-delete", "path={path}", path);

        var fileClient = GetFileClient(path);
        _logger.LogDebug("Deleting to {path}", path);

        try
        {
            var conditions = new DataLakeRequestConditions
            {
                LeaseId = leaseId
            };

            Response<bool> response = await fileClient.DeleteIfExistsAsync(conditions);
            if (!response.Value)
            {
                _logger.LogDebug("File path={path} does not exist", fileClient.Path);
                return StatusCode.NotFound;
            }

            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode != null && _statusErrors.TryGetValue(ex.ErrorCode, out var statusError))
        {
            _logger.LogError(ex, "Failed to delete file {path}, {error}", fileClient.Path, ex.ErrorCode);
            return statusError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {path}", fileClient.Path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<Option> Exists(string key)
    {
        key.NotEmpty();
        var fileClient = GetFileClient(key);

        Response<bool> exist = await fileClient.ExistsAsync();
        if (!exist.HasValue || !exist.Value)
        {
            _logger.LogDebug("File does not exist, path={path}", fileClient.Path);
            return StatusCode.NotFound;
        }

        return StatusCode.OK;
    }

    public async Task<Option<DataETag>> Get(string path)
    {
        path.NotEmpty();
        _logger.LogDebug("Getting file key={key}", path);
        var fileClient = GetFileClient(path);
        using var metric = _logger.LogDuration("dataLakeStore-read", "key={key}", path);

        try
        {
            var ifExists = await fileClient.ExistsAsync();
            metric.Log("InternalRead");
            if (ifExists.Value == false)
            {
                _logger.LogDebug("File not found, path={path}", fileClient.Path);
                return StatusCode.NotFound;
            }

            Response<FileDownloadInfo> response = await fileClient.ReadAsync();

            if (response.Value == null) return StatusCode.NotFound;
            metric.Log("readAsync");

            var expectedLength = response.Value.ContentLength;
            using MemoryStream memory = expectedLength > 0 && expectedLength <= int.MaxValue
                ? new MemoryStream((int)expectedLength)
                : new MemoryStream();

            await response.Value.Content.CopyToAsync(memory);

            byte[] data = memory.ToArray();
            string etag = response.Value.Properties.ETag.ToString();
            _logger.LogDebug("Read file {path}, size={size}, eTag={etag}", fileClient.Path, data.Length, etag);
            return new DataETag(data, etag);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            _logger.LogDebug("File not found {path}", fileClient.Path);
            return (StatusCode.NotFound, $"File not found, path={fileClient.Path}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file path={path}", fileClient.Path);
            return (StatusCode.BadRequest, ex.ToString());
        }
    }

    public async Task<Option<string>> Set(string path, DataETag data, string? leaseId = null)
    {
        path.NotEmpty();

        _logger.LogDebug("Getting file path={path}", path);
        var fileClient = GetFileClient(path);

        var setOption = await Upload(fileClient, true, data, leaseId);
        if (setOption.IsError()) return setOption;

        return setOption.Return();
    }

    private async Task<Option<string>> Upload(DataLakeFileClient fileClient, bool overwrite, DataETag dataETag, string? leaseId)
    {
        Response<PathInfo> result;
        _logger.LogDebug("Writing (Upload) to path={path}, length={length}, eTag={etag}", fileClient.Path, dataETag.Data.Length, dataETag.ETag?.ToString() ?? "<null>");
        dataETag.NotNull().Assert(x => x.Data.Length > 0, $"length must be greater then 0, path={fileClient.Path}");

        using var metric = _logger.LogDuration("dataLakeStore-upload", "path={path}", fileClient.Path);
        using var fromStream = new MemoryStream(dataETag.Data.Length);
        ReadOnlySpan<byte> span = dataETag.Data.AsSpan();
        fromStream.Write(span);
        fromStream.Position = 0;

        try
        {
            if (dataETag.ETag != default || leaseId.IsNotEmpty())
            {
                var option = new DataLakeFileUploadOptions
                {
                    Conditions = new DataLakeRequestConditions
                    {
                        IfMatch = leaseId.IsEmpty() && dataETag.ETag.IsNotEmpty() ? new ETag(dataETag.ETag) : null,
                        LeaseId = leaseId
                    }
                };

                result = await fileClient.UploadAsync(fromStream, option);
                return result.Value.ETag.ToString();
            }

            result = await fileClient.UploadAsync(fromStream, overwrite);
            return result.Value.ETag.ToString();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode != null && _statusErrors.TryGetValue(ex.ErrorCode, out var statusError))
        {
            logError(ex);
            return statusError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload {path}, exType={exType}, message={message}", fileClient.Path, ex.GetType(), ex.Message);
            return (StatusCode.InternalServerError, ex.Message.ToSafeLoggingFormat());
        }

        void logError(RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to upload, 'ErrorCode == {errorCode}', path={path}, message={message}", ex.ErrorCode, fileClient.Path, ex.Message);
        }
    }
}
