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
    public static async Task<Option<IFileLeasedAccess>> Acquire(this DataLakeFileClient fileClient, TimeSpan leaseDuration, ScopeContext context)
    {
        fileClient.NotNull();

        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        DataLakeLease? lease = null;

        try
        {
            lease = await leaseClient.AcquireAsync(leaseDuration);
            context.LogTrace("Lease acquired. Duration={duration}, leaseId={leaseId}", leaseDuration.ToString(), lease.LeaseId);
            return new DatalakeLeasedAccess(fileClient, leaseClient, context.Logger);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
        {
            context.LogTrace("Lease already present. Retrying...");
            return (StatusCode.Conflict, "Lease already present");
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
            return (StatusCode.Conflict, ex.Message);
        }
    }

    public static Task<Option<IFileLeasedAccess>> AcquireExclusive(this DataLakeFileClient fileClient, ScopeContext context)
        => Acquire(fileClient, TimeSpan.FromSeconds(-1), context);

    public static async Task<Option> Break(this DataLakeFileClient fileClient, ScopeContext context)
    {
        fileClient.NotNull();
        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();

        try
        {
            var result = await leaseClient.BreakAsync();
            if (result.GetRawResponse().IsError) return StatusCode.Conflict;

            context.LogTrace("Break lease");
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
            return (StatusCode.Conflict, ex.Message);
        }
    }

    public static Task<Option> Clear(ScopeContext context)
    {
        throw new NotImplementedException();
    }
}
