using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public static async Task<Option<IFileLeasedAccess>> Acquire(this DataLakeFileClient fileClient, TimeSpan leaseDuration, TimeSpan timeOut, ScopeContext context)
    {
        fileClient.NotNull();

        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        DataLakeLease? lease = null;
        DateTime runUntil = DateTime.UtcNow + timeOut;

        // Acquire the lease
        //while (lease == null && !context.CancellationToken.IsCancellationRequested && DateTime.UtcNow < runUntil)
        //{
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
            //await Task.Delay(TimeSpan.FromSeconds(1));
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
            return (StatusCode.Conflict, ex.Message);
        }
        //}

        //context.LogWarning("Failed to acquire lease and stopped trying");
        //return (StatusCode.Conflict, "Failed to acquire lease and stopped trying");
    }

    public static Task<Option<IFileLeasedAccess>> AcquireExclusive(this DataLakeFileClient fileClient, TimeSpan timeOut, ScopeContext context)
        => Acquire(fileClient, TimeSpan.FromSeconds(-1), timeOut, context);

    public static Task<Option<IFileLeasedAccess>> Break(TimeSpan leaseDuration, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public static Task<Option> Clear(ScopeContext context)
    {
        throw new NotImplementedException();
    }
}
