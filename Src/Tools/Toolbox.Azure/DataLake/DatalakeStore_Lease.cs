using System.Security.Cryptography;
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
    private const string _leaseAlreadyPresentText = "LeaseAlreadyPresent";
    private const string _blobNotFoundText = "BlobNotFound";
    private static readonly TimeSpan _leaseRetryDuration = TimeSpan.FromSeconds(30);

    public async Task<Option> BreakLease(string path)
    {
        path.NotEmpty();

        var fileClient = GetFileClient(path);
        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        _logger.LogDebug("Attempting to break lease for path={path}, leaseId={leaseId}", fileClient.Path, leaseClient.LeaseId);

        try
        {
            var result = await leaseClient.BreakAsync();
            if (result.GetRawResponse().IsError)
            {
                _logger.LogError("Failed to break lease, reason={reason}", result.GetRawResponse().ToString());
                return (StatusCode.Conflict, "Failed to break lease");
            }

            _logger.LogWarning("Lease has been broken");
            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == _blobNotFoundText)
        {
            _logger.LogDebug("Blob not found while breaking lease for path={path}", fileClient.Path);
            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseNotPresentWithLeaseOperation" || ex.ErrorCode == "LeaseNotPresent")
        {
            _logger.LogDebug("Lease already absent for path={path}", fileClient.Path);
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to break lease on path={path}", fileClient.Path);
            return (StatusCode.Conflict, ex.Message);
        }
    }

    public async Task<Option<string>> AcquireExclusiveLock(string path, bool breakLeaseIfExist)
    {
        var leaseDuration = TimeSpan.FromSeconds(-1);

        var fileClient = GetFileClient(path);
        var acquireOption = await InternalAcquireLease(fileClient, leaseDuration);
        if (acquireOption.IsOk()) return acquireOption;
        if (acquireOption.IsLocked() && !breakLeaseIfExist) return acquireOption;

        _logger.LogWarning("Failed to acquire lease, attempting to break lease");
        var breakOption = await BreakLease(path);
        if (breakOption.IsError()) return breakOption.ToOptionStatus<string>();

        acquireOption = await InternalAcquireLease(fileClient, leaseDuration);
        return acquireOption;
    }

    public async Task<Option<string>> AcquireLease(string path, TimeSpan leaseDuration)
    {
        path.NotEmpty();

        var fileClient = GetFileClient(path);
        var acquireOption = await InternalAcquireLease(fileClient, leaseDuration);
        return acquireOption;
    }

    public async Task<Option> ReleaseLease(string path, string leaseId)
    {
        path.NotEmpty();
        leaseId.NotEmpty();

        var leaseClient = GetFileClient(path).GetDataLakeLeaseClient(leaseId);

        try
        {
            _logger.LogDebug("Releasing lease for path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);

            Response<ReleasedObjectInfo> result = await leaseClient.ReleaseAsync();
            if (!result.HasValue)
            {
                _logger.LogError("Failed to release lease leaseId={leaseId} on path={path}", leaseClient.LeaseId, path);
                return StatusCode.Conflict;
            }
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing")
        {
            _logger.LogDebug("(LeaseIdMissing) Invalid lease Id path={path}", path);
            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            _logger.LogDebug("(BlobNotFound) Invalid lease Id path={path}", path);
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ignore error, failed to release lease on path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);
        }

        _logger.LogDebug("Released lease for path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);
        return StatusCode.OK;
    }

    public async Task<Option> RenewLease(string key, string leaseId)
    {
        key.NotEmpty();
        leaseId.NotEmpty();

        var leaseClient = GetFileClient(key).GetDataLakeLeaseClient(leaseId);

        try
        {
            _logger.LogDebug("Renewing lease for path={path}, leaseId={leaseId}", key, leaseId);

            Response<DataLakeLease> result = await leaseClient.RenewAsync();
            if (!result.HasValue || result.Value == null || result.Value.LeaseId.IsEmpty())
            {
                _logger.LogError("Failed to renew lease on path={path}, leaseId={leaseId}", key, leaseId);
                return StatusCode.Conflict;
            }

            _logger.LogDebug("Renewed lease for path={path}, leaseId={leaseId}", key, result.Value.LeaseId);
            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == _blobNotFoundText)
        {
            _logger.LogDebug("(BlobNotFound) Cannot renew lease, path={path}", key);
            return StatusCode.NotFound;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseNotPresentWithLeaseOperation" || ex.ErrorCode == "LeaseNotPresent" || ex.ErrorCode == "LeaseIdMismatchWithLeaseOperation")
        {
            _logger.LogDebug("(LeaseNotPresent) Cannot renew lease, path={path}", key);
            return StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to renew lease on path={path}, leaseId={leaseId}", key, leaseId);
            return (StatusCode.InternalServerError, ex.Message);
        }
    }

    // Returns the LeaseID
    private async Task<Option<string>> InternalAcquireLease(DataLakeFileClient fileClient, TimeSpan leaseDuration)
    {
        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        DataLakeLease? lease = null;
        int notFoundCount = 0;

        var scopeToken = new CancellationTokenSource(_leaseRetryDuration);
        while (!scopeToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Attempting to acquire lease, leaseDuration={leaseDuration}", leaseDuration.ToString());
                lease = await leaseClient.AcquireAsync(leaseDuration);

                _logger.LogDebug("Lease acquired. Duration={duration}, leaseId={leaseId}", leaseDuration.ToString(), lease.LeaseId);
                return leaseClient.LeaseId;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == _blobNotFoundText && notFoundCount++ == 0)
            {
                Response<PathInfo> leaseResult = await fileClient.CreateIfNotExistsAsync(PathResourceType.File);
                leaseResult.NotNull();
                if (!leaseResult.HasValue)
                {
                    _logger.LogError(ex, "Failed to acquire lease");
                    return (StatusCode.Conflict, ex.Message);
                }

                continue;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == _leaseAlreadyPresentText)
            {
                _logger.LogWarning("Lease already present. Retrying...");
                var waitPeriod = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(300));
                await Task.Delay(waitPeriod);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acquire lease");
                return (StatusCode.Conflict, ex.Message);
            }
        }

        _logger.LogError("Failed to acquire lease, timed out duration={duration}", _leaseRetryDuration.ToString());
        return (StatusCode.Locked, _leaseAlreadyPresentText);
    }
}
