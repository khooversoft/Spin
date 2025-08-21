﻿using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public class DatalakeLeasedAccess : IFileLeasedAccess
{
    private readonly DataLakeLeaseClient _leaseClient;
    private readonly ILogger _logger;
    private readonly DataLakeFileClient _fileClient;
    private bool _disposed = false;

    public DatalakeLeasedAccess(DataLakeFileClient fileClient, DataLakeLeaseClient leaseClient, ILogger logger)
    {
        _fileClient = fileClient.NotNull();
        _leaseClient = leaseClient.NotNull();
        _logger = logger.NotNull();
    }

    public string Path => _fileClient.Path;
    public string LeaseId => _leaseClient.LeaseId.NotEmpty();

    public Task<Option<string>> Append(DataETag data, ScopeContext context) => _fileClient.Append(this, data, context);
    public Task<Option<DataETag>> Get(ScopeContext context) => _fileClient.Get(this, context);
    public Task<Option<string>> Set(DataETag data, ScopeContext context) => _fileClient.Set(this, data, context);

    public async Task<Option> Renew(ScopeContext context)
    {
        Response<DataLakeLease> result = await _leaseClient.RenewAsync();
        if (!result.HasValue || result.Value == null || result.Value.LeaseId.IsEmpty())
        {
            context.LogError("Failed to acquire lease on path={path}", Path);
            return StatusCode.Conflict;
        }

        context.LogDebug("Renewed lease for path={path}, oldLeaseId={oldLeaseId}, newLeaseId={newLeaseId}", Path, LeaseId, result.Value.LeaseId);
        return StatusCode.OK;
    }

    public async Task<Option> Release(ScopeContext context)
    {
        context = context.With(_logger);

        try
        {
            context.Location().LogDebug("Releasing lease for path={path}, leaseId={leaseId}", Path, LeaseId);

            Response<ReleasedObjectInfo> result = await _leaseClient.ReleaseAsync();
            if (!result.HasValue)
            {
                context.LogError("Failed to release lease on path={path}", Path);
                return StatusCode.Conflict;
            }
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing")
        {
            context.LogDebug("Invalid lease Id path={path}", Path);
            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Ignore error, failed to release lease on path={path}, leaseId={leaseId}", Path, LeaseId);
        }

        Interlocked.Exchange(ref _disposed, true);

        context.LogDebug("Released lease for path={path}, leaseId={leaseId}", Path, LeaseId);
        return StatusCode.OK;
    }

    public async ValueTask DisposeAsync()
    {
        var context = _logger.ToScopeContext();

        bool disposed = Interlocked.Exchange(ref _disposed, true);
        if (!disposed)
        {
            context.Location().LogDebug("DisposingAsync release lease for path={path}, leaseId={leaseId}", Path, LeaseId);
            await Release(context);
        }
    }
}
