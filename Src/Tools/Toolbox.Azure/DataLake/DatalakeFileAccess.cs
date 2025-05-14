using Azure;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public class DatalakeFileAccess : IFileAccess
{
    private readonly ILogger _logger;
    private readonly DataLakeFileClient _fileClient;

    public DatalakeFileAccess(DataLakeFileClient fileClient, ILogger logger)
    {
        _fileClient = fileClient.NotNull();
        _logger = logger.NotNull();
    }

    public string Path => _fileClient.Path;

    public async Task<Option<string>> Add(DataETag data, ScopeContext context)
    {
        var result = await _fileClient.Add(data, context);
        if (result.IsError()) return result;

        return result.Return().ToString();
    }

    public Task<Option<string>> Append(DataETag data, ScopeContext context) => _fileClient.Append(data, context);

    public async Task<Option> Delete(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-delete", "path={path}", Path);

        context.LogDebug("Deleting to {path}", _fileClient.Path);

        try
        {
            Response<bool> response = await _fileClient.DeleteIfExistsAsync(cancellationToken: context).ConfigureAwait(false);

            if (!response.Value)
            {
                context.LogDebug("File path={path} does not exist", _fileClient.Path);
                return StatusCode.NotFound;
            }

            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing")
        {
            context.Location().LogError(ex, "Failed to delete file {path}, LeaseIdMissing", _fileClient.Path);
            return StatusCode.Locked;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to delete file {path}", _fileClient.Path);
            return StatusCode.BadRequest;
        }
    }

    public async Task<Option> Exist(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-exist", "path={path}", _fileClient.Path);

        context.LogDebug("Is path {path} exist", _fileClient.Path);

        try
        {
            Response<bool> response = await _fileClient.ExistsAsync(context).ConfigureAwait(false);
            return response.Value ? StatusCode.OK : StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to ExistsAsync for {path}", _fileClient.Path);
            throw;
        }
    }

    public Task<Option<DataETag>> Get(ScopeContext context) => _fileClient.Get(context);
    public Task<Option<IStorePathDetail>> GetDetail(ScopeContext context) => _fileClient.GetPathDetail(context);
    public Task<Option<string>> Set(DataETag data, ScopeContext context) => _fileClient.Set(data, context);
    public Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, ScopeContext context) => _fileClient.Acquire(leaseDuration, context);
    public Task<Option<IFileLeasedAccess>> AcquireExclusive(bool breakLeaseIfExist, ScopeContext context) => _fileClient.AcquireExclusive(breakLeaseIfExist, context);
    public Task<Option> BreakLease(ScopeContext context) => DatalakeLeaseTool.Break(_fileClient, context);
}
