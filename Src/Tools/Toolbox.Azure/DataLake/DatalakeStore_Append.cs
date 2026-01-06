using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public partial class DatalakeStore
{
    private readonly AsyncKeyGuard _appendGuard = new();

    public async Task<Option<string>> Append(string path, DataETag data, string? leaseId = null)
    {
        path.NotEmpty();
        data.NotNull().Assert(x => x.Data.Length > 0, $"{nameof(data)} length must be greater then 0, path={path}");
        var fileClient = GetFileClient(path);

        using var metric = _logger.LogDuration("dataLakeStore-append", "path={path}, dataSize={dataSize}", fileClient.Path, data.Data.Length);
        _logger.LogDebug("Appending to {path}, data.Length={data.Length}", fileClient.Path, data.Data.Length);

        using var guardScope = await _appendGuard.AcquireLock(path);

        var resultOption = leaseId switch
        {
            null => await getLeaseAndAppend(),
            string v => await LeaseAppend(fileClient, leaseId, data),
        };

        return resultOption;


        async Task<Option<string>> getLeaseAndAppend()
        {
            _logger.LogDebug("Acquiring lease for path={path}", fileClient.Path);

            var leaseOption = await InternalAcquireLease(fileClient, TimeSpan.FromSeconds(30));
            if (leaseOption.IsError())
            {
                _logger.LogError("Failed to acquire lease for path={path}", fileClient.Path);
                return leaseOption;
            }

            var leaseId = leaseOption.Return().NotEmpty();
            return await LeaseAppend(fileClient, leaseId, data);
        }
    }

    private async Task<Option<string>> LeaseAppend(DataLakeFileClient fileClient, string leaseId, DataETag data)
    {
        var fileAppendOption = new DataLakeFileAppendOptions { LeaseId = leaseId };
        var flushOptions = new DataLakeFileFlushOptions { Conditions = new DataLakeRequestConditions { LeaseId = leaseId } };

        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

        try
        {
            Option<StorePathDetail> pathDetailOption = await GetPathDetailOrCreate(fileClient);
            if (pathDetailOption.IsError()) return pathDetailOption.ToOptionStatus<string>();
            var pathDetail = pathDetailOption.Return();

            _logger.LogDebug("Appending to file with options, path={path}, leaseId={leaseId}", fileClient.Path, fileAppendOption.LeaseId);
            await fileClient.AppendAsync(memoryBuffer, pathDetail.ContentLength, fileAppendOption);

            Response<PathInfo> resultLock = await fileClient.FlushAsync(pathDetail.ContentLength + data.Data.Length, options: flushOptions);
            if (!resultLock.HasValue)
            {
                _logger.LogDebug("Failed to flush data, path={path}", fileClient.Path);
                return (StatusCode.Conflict, "Failed to flush data");
            }

            return resultLock.Value.ETag.ToString();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
        {
            _logger.LogDebug("Creating path={path}", fileClient.Path);
            return await Upload(fileClient, true, data, null);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode != null && _statusErrors.TryGetValue(ex.ErrorCode, out var statusError))
        {
            _logger.LogError(ex, "Failed to append file {path}, {error}", fileClient.Path, ex.ErrorCode);
            return statusError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append file {path}", fileClient.Path);
            return (StatusCode.BadRequest, $"Failed to append file {fileClient.Path}");
        }
    }

    private async Task<Option<StorePathDetail>> GetPathDetailOrCreate(DataLakeFileClient fileClient)
    {
        using var metric = _logger.LogDuration("dataLakeStore-getPathPropertiesOrCreate");

        var properties = await fileClient.GetPathDetail(_logger);
        if (properties.IsOk()) return properties;

        await fileClient.CreateIfNotExistsAsync(PathResourceType.File);
        return await fileClient.GetPathDetail(_logger);
    }
}
