using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public readonly struct DatalakeLease : IDatalakeLease
{
    public DatalakeLease(DataLakeFileSystemClient fileSystemClient, string path, string leaseId)
    {
        FileSystemClient = fileSystemClient.NotNull();
        Path = path.NotEmpty();
        LeaseId = leaseId.NotEmpty();
    }

    public DataLakeFileSystemClient FileSystemClient { get; }
    public string Path { get; }
    public string LeaseId { get; }
    public DateTime DateAcquired { get; } = DateTime.UtcNow;
    public TimeSpan Elapsed => DateTime.UtcNow - DateAcquired;
    public bool ShouldRenew => Elapsed > TimeSpan.FromSeconds(30);

    public Task<Option> Release(ScopeContext context) => this.ReleaseLease(context);
    public Task<Option<IDatalakeLease>> Renew(ScopeContext context) => this.RenewLease(context);

    public DatalakeLease WithLeaseId(string leaseId) => new DatalakeLease(FileSystemClient, Path, leaseId);
}


public static class DatalakeLeaseTool
{
    public static async Task<Option<IDatalakeLease>> AcquireLease(this DataLakeFileSystemClient systemClient, string path, TimeSpan duration, ScopeContext context)
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
                return new DatalakeLease(systemClient, path, lease.LeaseId);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
            {
                context.LogTrace("Lease already present. Retrying...");
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
                return (StatusCode.Conflict, ex.Message);
            }
        }

        context.LogWarning("Failed to acquire lease and stopped trying");
        return StatusCode.NoContent;
    }

    public static async Task<Option<IDatalakeLease>> RenewLease(this DatalakeLease datalakeLease, ScopeContext context)
    {
        DataLakeLeaseClient leaseClient = datalakeLease.FileSystemClient.GetDataLakeLeaseClient(datalakeLease.LeaseId);

        Response<DataLakeLease> result = await leaseClient.RenewAsync();
        if (!result.HasValue || result.Value == null || result.Value.LeaseId.IsEmpty())
        {
            context.LogError("Failed to acquire lease on path={path}", datalakeLease.Path);
            return StatusCode.Conflict;
        }

        context.LogTrace("Renewed lease for path={path}", datalakeLease.Path);
        return datalakeLease.WithLeaseId(result.Value.LeaseId);
    }

    public static async Task<Option> ReleaseLease(this DatalakeLease datalakeLease, ScopeContext context)
    {
        DataLakeLeaseClient leaseClient = datalakeLease.FileSystemClient.GetDataLakeLeaseClient(datalakeLease.LeaseId);

        Response<ReleasedObjectInfo> result = await leaseClient.ReleaseAsync();
        if (!result.HasValue)
        {
            context.LogError("Failed to release lease on path={path}", datalakeLease.Path);
            return StatusCode.Conflict;
        }

        context.LogTrace("Released lease for path={path}", datalakeLease.Path);
        return StatusCode.OK;
    }
}
