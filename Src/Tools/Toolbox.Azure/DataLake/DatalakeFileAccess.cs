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

        context.Location().LogTrace("Deleting to {path}", _fileClient.Path);

        try
        {
            Response<bool> response = await _fileClient.DeleteIfExistsAsync(cancellationToken: context).ConfigureAwait(false);

            if (!response.Value) context.Location().LogTrace("File path={path} does not exist", _fileClient.Path);

            return response.Value ? StatusCode.OK : StatusCode.NotFound;
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

        context.Location().LogTrace("Is path {path} exist", _fileClient.Path);

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

    public Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, ScopeContext context) => DatalakeLeaseTool.Acquire(_fileClient, leaseDuration, context);
    public Task<Option<IFileLeasedAccess>> AcquireExclusive(ScopeContext context) => DatalakeLeaseTool.Acquire(_fileClient, TimeSpan.FromSeconds(-1), context);
    public Task<Option> BreakLease(ScopeContext context) => DatalakeLeaseTool.Break(_fileClient, context);
}
