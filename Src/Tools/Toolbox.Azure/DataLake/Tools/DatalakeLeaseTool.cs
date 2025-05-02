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
    private static readonly TimeSpan DefaultLeaseDuration = TimeSpan.FromSeconds(5);

    public static async Task<Option<IFileLeasedAccess>> Acquire(this DataLakeFileClient fileClient, TimeSpan leaseDuration, ScopeContext context)
    {
        var acquireOption = await fileClient.InternalAcquire(leaseDuration, context).ConfigureAwait(false);
        return acquireOption;
    }

    public static async Task<Option<IFileLeasedAccess>> AcquireExclusive(this DataLakeFileClient fileClient, bool breakLeaseIfExist, ScopeContext context)
    {
        var leaseDuration = TimeSpan.FromSeconds(-1);

        var acquireOption = await fileClient.InternalAcquire(leaseDuration, context).ConfigureAwait(false);
        if (acquireOption.IsOk()) return acquireOption;
        if (acquireOption.IsLocked() && !breakLeaseIfExist) return acquireOption;

        var breakOption = await fileClient.Break(context).ConfigureAwait(false);
        if (breakOption.IsError()) return breakOption.ToOptionStatus<IFileLeasedAccess>();

        acquireOption = await fileClient.InternalAcquire(leaseDuration, context).ConfigureAwait(false);
        return acquireOption;
    }

    public static async Task<Option> Break(this DataLakeFileClient fileClient, ScopeContext context)
    {
        fileClient.NotNull();
        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();

        try
        {
            var result = await leaseClient.BreakAsync();
            if (result.GetRawResponse().IsError) return (StatusCode.Conflict, "Failed to break lease");

            context.LogTrace("Break lease");
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
            return (StatusCode.Conflict, ex.Message);
        }
    }

    private static async Task<Option<IFileLeasedAccess>> InternalAcquire(this DataLakeFileClient fileClient, TimeSpan leaseDuration, ScopeContext context)
    {
        fileClient.NotNull();

        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        DataLakeLease? lease = null;
        int notFoundCount = 0;

        var token = new CancellationTokenSource(DefaultLeaseDuration);
        while (!token.IsCancellationRequested)
        {
            try
            {
                lease = await leaseClient.AcquireAsync(leaseDuration);
                context.LogTrace("Lease acquired. Duration={duration}, leaseId={leaseId}", leaseDuration.ToString(), lease.LeaseId);
                return new DatalakeLeasedAccess(fileClient, leaseClient, context.Logger);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == _blobNotFoundText && notFoundCount++ == 0)
            {
                Response<PathInfo> leaseResult = await fileClient.CreateIfNotExistsAsync(PathResourceType.File, cancellationToken: token.Token).ConfigureAwait(false);
                if (!leaseResult.HasValue)
                {
                    context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
                    return (StatusCode.Conflict, ex.Message);
                }

                continue;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == _leaseAlreadyPresentText)
            {
                context.LogTrace("Lease already present. Retrying...");

                var waitPeriod = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(300));
                await Task.Delay(waitPeriod, context.CancellationToken).ConfigureAwait(false);
                continue;
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
                return (StatusCode.Conflict, ex.Message);
            }
        }

        return (StatusCode.Locked, _leaseAlreadyPresentText);
    }
}
