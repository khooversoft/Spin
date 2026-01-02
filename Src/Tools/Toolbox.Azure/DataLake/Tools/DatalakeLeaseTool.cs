//using System.Security.Cryptography;
//using Azure;
//using Azure.Storage.Files.DataLake;
//using Azure.Storage.Files.DataLake.Models;
//using Microsoft.Extensions.Logging;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Azure;

//public static class DatalakeLeaseTool
//{
//    private const string _leaseAlreadyPresentText = "LeaseAlreadyPresent";
//    private const string _blobNotFoundText = "BlobNotFound";
//    private static readonly TimeSpan _leaseRetryDuration = TimeSpan.FromSeconds(5);

//    public static async Task<Option<string>> AcquireLease(this DataLakeFileClient fileClient, TimeSpan leaseDuration, ILogger logger)
//    {
//        var acquireOption = await fileClient.InternalAcquireLease(leaseDuration, logger);
//        return acquireOption;
//    }

//    public static async Task<Option<string>> AcquireExclusiveLease(this DataLakeFileClient fileClient, bool breakLeaseIfExist, ILogger logger)
//    {
//        var leaseDuration = TimeSpan.FromSeconds(-1);

//        var acquireOption = await fileClient.InternalAcquireLease(leaseDuration, logger);
//        if (acquireOption.IsOk()) return acquireOption;
//        if (acquireOption.IsLocked() && !breakLeaseIfExist) return acquireOption;

//        logger.LogWarning("Failed to acquire lease, attempting to break lease");
//        var breakOption = await fileClient.Break(logger);
//        if (breakOption.IsError()) return breakOption.ToOptionStatus<string>();

//        acquireOption = await fileClient.InternalAcquireLease(leaseDuration, logger);
//        return acquireOption;
//    }

//    public static async Task<Option> Break(this DataLakeFileClient fileClient, ILogger logger)
//    {
//        fileClient.NotNull();
//        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
//        logger.LogDebug("Attempting to breaking lease for path={path}, leaseId={leaseId}", fileClient.Path, leaseClient.LeaseId);

//        try
//        {
//            var result = await leaseClient.BreakAsync();
//            if (result.GetRawResponse().IsError)
//            {
//                logger.LogError("Failed to break lease, reason={reason}", result.GetRawResponse().ToString());
//                return (StatusCode.Conflict, "Failed to break lease");
//            }

//            logger.LogWarning("Lease has been broken");
//            return StatusCode.OK;
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Failed to acquire lease");
//            return (StatusCode.Conflict, ex.Message);
//        }
//    }

//    // Returns the LeaseID
//    internal static async Task<Option<string>> InternalAcquireLease(this DataLakeFileClient fileClient, TimeSpan leaseDuration, ILogger logger, CancellationToken token = default)
//    {
//        logger.NotNull();

//        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
//        DataLakeLease? lease = null;
//        int notFoundCount = 0;

//        var scopeToken = CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(_leaseRetryDuration).Token, token);
//        while (!scopeToken.IsCancellationRequested)
//        {
//            try
//            {
//                logger.LogDebug("Attempting to acquire lease, leaseDuration={leaseDuration}", leaseDuration.ToString());
//                lease = await leaseClient.AcquireAsync(leaseDuration, cancellationToken: token);

//                logger.LogDebug("Lease acquired. Duration={duration}, leaseId={leaseId}", leaseDuration.ToString(), lease.LeaseId);
//                return leaseClient.LeaseId;
//            }
//            catch (RequestFailedException ex) when (ex.ErrorCode == _blobNotFoundText && notFoundCount++ == 0)
//            {
//                Response<PathInfo> leaseResult = await fileClient.CreateIfNotExistsAsync(PathResourceType.File, cancellationToken: token.Token);
//                if (!leaseResult.HasValue)
//                {
//                    logger.LogError(ex, "Failed to acquire lease");
//                    return (StatusCode.Conflict, ex.Message);
//                }

//                continue;
//            }
//            catch (RequestFailedException ex) when (ex.ErrorCode == _leaseAlreadyPresentText)
//            {
//                logger.LogWarning("Lease already present. Retrying...");
//                var waitPeriod = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(300));
//                await Task.Delay(waitPeriod);
//                continue;
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Failed to acquire lease");
//                return (StatusCode.Conflict, ex.Message);
//            }
//        }

//        logger.LogError("Failed to acquire lease, timed out duration={duration}", _leaseRetryDuration.ToString());
//        return (StatusCode.Locked, _leaseAlreadyPresentText);
//    }

//    internal static async Task<Option> ReleaseLease(this DataLakeLeaseClient leaseClient, string path, ILogger logger, CancellationToken token = default)
//    {
//        leaseClient.NotNull();
//        path.NotEmpty();
//        logger.NotNull();

//        try
//        {
//            logger.LogDebug("Releasing lease for path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);

//            Response<ReleasedObjectInfo> result = await leaseClient.ReleaseAsync();
//            if (!result.HasValue)
//            {
//                logger.LogError("Failed to release lease leaseId={leaseId} on path={path}", leaseClient.LeaseId, path);
//                return StatusCode.Conflict;
//            }
//        }
//        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing")
//        {
//            logger.LogDebug("(LeaseIdMissing) Invalid lease Id path={path}", path);
//            return StatusCode.OK;
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Ignore error, failed to release lease on path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);
//        }

//        logger.LogDebug("Released lease for path={path}, leaseId={leaseId}", path, leaseClient.LeaseId);
//        return StatusCode.OK;
//    }
//}
