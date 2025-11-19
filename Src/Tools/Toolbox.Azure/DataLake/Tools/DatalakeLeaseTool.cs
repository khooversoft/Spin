using System.Security.Cryptography;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class DatalakeLeaseTool
{
    private const string _leaseAlreadyPresentText = "LeaseAlreadyPresent";
    private const string _blobNotFoundText = "BlobNotFound";
    private static readonly TimeSpan _leaseRetryDuration = TimeSpan.FromSeconds(5);

    public static async Task<Option<IFileLeasedAccess>> AcquireLease(this DataLakeFileClient fileClient, DatalakeStore datalakeStore, TimeSpan leaseDuration, ScopeContext context)
    {
        var acquireOption = await fileClient.InternalAcquire(datalakeStore, leaseDuration, context);
        return acquireOption;
    }

    public static async Task<Option<IFileLeasedAccess>> AcquireExclusiveLease(this DataLakeFileClient fileClient, DatalakeStore datalakeStore, bool breakLeaseIfExist, ScopeContext context)
    {
        var leaseDuration = TimeSpan.FromSeconds(-1);

        var acquireOption = await fileClient.InternalAcquire(datalakeStore, leaseDuration, context);
        if (acquireOption.IsOk()) return acquireOption;
        if (acquireOption.IsLocked() && !breakLeaseIfExist) return acquireOption;

        context.LogWarning("Failed to acquire lease, attempting to break lease");
        var breakOption = await fileClient.Break(context);
        if (breakOption.IsError()) return breakOption.ToOptionStatus<IFileLeasedAccess>();

        acquireOption = await fileClient.InternalAcquire(datalakeStore, leaseDuration, context);
        return acquireOption;
    }

    public static async Task<Option> Break(this DataLakeFileClient fileClient, ScopeContext context)
    {
        fileClient.NotNull();
        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        context.LogDebug("Attempting to breaking lease for path={path}, leaseId={leaseId}", fileClient.Path, leaseClient.LeaseId);

        try
        {
            var result = await leaseClient.BreakAsync();
            if (result.GetRawResponse().IsError)
            {
                context.LogError("Failed to break lease, reason={reason}", result.GetRawResponse().ToString());
                return (StatusCode.Conflict, "Failed to break lease");
            }

            context.LogWarning("Lease has been broken");
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
            return (StatusCode.Conflict, ex.Message);
        }
    }

    private static async Task<Option<IFileLeasedAccess>> InternalAcquire(this DataLakeFileClient fileClient, DatalakeStore datalakeStore, TimeSpan leaseDuration, ScopeContext context)
    {
        fileClient.NotNull();

        var leaseOption = await fileClient.InternalAcquireLease(leaseDuration, context);
        if (leaseOption.IsError()) return leaseOption.ToOptionStatus<IFileLeasedAccess>();

        DataLakeLeaseClient leaseClient = leaseOption.Return();
        return new DatalakeLeasedAccess(datalakeStore, fileClient, leaseClient, context.Logger);
    }

    internal static async Task<Option<DataLakeLeaseClient>> InternalAcquireLease(this DataLakeFileClient fileClient, TimeSpan leaseDuration, ScopeContext context)
    {
        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        DataLakeLease? lease = null;
        int notFoundCount = 0;

        var token = new CancellationTokenSource(_leaseRetryDuration);
        while (!token.IsCancellationRequested)
        {
            try
            {
                context.LogDebug("Attempting to acquire lease, leaseDuration={leaseDuration}", leaseDuration.ToString());
                lease = await leaseClient.AcquireAsync(leaseDuration);

                context.LogDebug("Lease acquired. Duration={duration}, leaseId={leaseId}", leaseDuration.ToString(), lease.LeaseId);
                return leaseClient;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == _blobNotFoundText && notFoundCount++ == 0)
            {
                Response<PathInfo> leaseResult = await fileClient.CreateIfNotExistsAsync(PathResourceType.File, cancellationToken: token.Token);
                if (!leaseResult.HasValue)
                {
                    context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
                    return (StatusCode.Conflict, ex.Message);
                }

                continue;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == _leaseAlreadyPresentText)
            {
                context.LogWarning("Lease already present. Retrying...");
                var waitPeriod = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(300));
                await Task.Delay(waitPeriod, context.CancellationToken);
                continue;
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Failed to acquire lease");
                return (StatusCode.Conflict, ex.Message);
            }
        }

        context.LogError("Failed to acquire lease, timed out duration={duration}", _leaseRetryDuration.ToString());
        return (StatusCode.Locked, _leaseAlreadyPresentText);
    }

    internal static async Task<Option> ReleaseLease(this DataLakeLeaseClient leaseClient, string path, ScopeContext context)
    {
        leaseClient.NotNull();
        path.NotEmpty();

        try
        {
            context.Location().LogDebug("Releasing lease for path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);

            Response<ReleasedObjectInfo> result = await leaseClient.ReleaseAsync();
            if (!result.HasValue)
            {
                context.LogError("Failed to release lease leaseId={leaseId} on path={path}", leaseClient.LeaseId, path);
                return StatusCode.Conflict;
            }
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing")
        {
            context.LogDebug("(LeaseIdMissing) Invalid lease Id path={path}", path);
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Ignore error, failed to release lease on path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);
        }

        context.LogDebug("Released lease for path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);
        return StatusCode.OK;
    }
}
