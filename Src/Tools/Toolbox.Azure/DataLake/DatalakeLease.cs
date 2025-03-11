using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public readonly struct DatalakeLease
{
    public DatalakeLease(string path, DataLakeLease dataLakeLease)
    {
        Path = path.NotNull();
        DataLakeLease = dataLakeLease.NotNull();
    }

    public string Path { get; }
    public DataLakeLease DataLakeLease { get; }
}


public static class DatalakeLeaseTool
{
    public static async Task<Option<DatalakeLease>> AcquireLease(this DataLakeFileSystemClient systemClient, string path, TimeSpan duration, ScopeContext context)
    {
        var fileClient = systemClient.GetFileClient(path);

        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
        DataLakeLease? lease = null;

        // Acquire the lease
        while (lease == null && !context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                lease = await leaseClient.AcquireAsync(duration);
                context.LogTrace("Lease acquired. Lease ID={leaseId}", lease.LeaseId);
                return new DatalakeLease(path, lease);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
            {
                context.LogTrace("Lease already present. Retrying...");
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Failed to acquire lease, {isCancellationRequested", context.CancellationToken.IsCancellationRequested);
                return new Option<DatalakeLease>(StatusCode.InternalServerError, ex.Message);
            }
        }

        return StatusCode.NoContent;
    }
}
