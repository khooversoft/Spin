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
    private readonly DatalakeStore _datalakeStore;

    public DatalakeFileAccess(DatalakeStore datalakeStore, DataLakeFileClient fileClient, ILogger logger)
    {
        _fileClient = fileClient.NotNull();
        _logger = logger.NotNull();
        _datalakeStore = datalakeStore.NotNull();
    }

    public string Path => _fileClient.Path;

    public async Task<Option<string>> Add(DataETag data, ScopeContext context)
    {
        var result = await _fileClient.Add(data, context);
        if (result.IsError()) return result;

        //_datalakeStore.DataChangeLog.GetRecorder()?.Add(Path, data);
        return result.Return();
    }

    public Task<Option<string>> Append(DataETag data, ScopeContext context)
    {
        _datalakeStore.DataChangeLog.GetRecorder().Assert(x => x == null, "Append is not supported with DataChangeRecorder");
        return _fileClient.Append(data, context);
    }

    public async Task<Option> Delete(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-delete", "path={path}", Path);

        context.LogDebug("Deleting to {path}", _fileClient.Path);

        try
        {
            Option<DataETag> readOption = StatusCode.NotFound;

            if (_datalakeStore.DataChangeLog.GetRecorder() != null)
            {
                readOption = await _fileClient.Get(context);
                if (readOption.IsError()) return readOption.ToOptionStatus();
            }

            Response<bool> response = await _fileClient.DeleteIfExistsAsync(cancellationToken: context);

            if (!response.Value)
            {
                context.LogDebug("File path={path} does not exist", _fileClient.Path);
                return StatusCode.NotFound;
            }

            //_datalakeStore.DataChangeLog.GetRecorder()?.Add(Path, readOption.Return());
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

    public async Task<Option> Exists(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("dataLakeStore-exist", "path={path}", _fileClient.Path);

        context.LogDebug("Is path {path} exist", _fileClient.Path);

        try
        {
            Response<bool> response = await _fileClient.ExistsAsync(context);
            return response.Value ? StatusCode.OK : StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to ExistsAsync for {path}", _fileClient.Path);
            throw;
        }
    }

    public async Task<Option<string>> Set(DataETag data, ScopeContext context)
    {
        Option<DataETag> readOption = StatusCode.NotFound;

        if (_datalakeStore.DataChangeLog.GetRecorder() != null) readOption = await _fileClient.Get(context);

        var setOption = await _fileClient.Set(data, context);
        if (setOption.IsError()) return setOption;

        if (_datalakeStore.DataChangeLog.GetRecorder() != null)
        {
            //if (readOption.IsOk())
            //    _datalakeStore.DataChangeLog.GetRecorder()?.Update(Path, readOption.Return(), data);
            //else
            //    _datalakeStore.DataChangeLog.GetRecorder()?.Add(Path, readOption.Return());
        }

        return setOption.Return();
    }

    public Task<Option<DataETag>> Get(ScopeContext context) => _fileClient.Get(context);
    public Task<Option<StorePathDetail>> GetDetails(ScopeContext context) => _fileClient.GetPathDetail(context);
    public Task<Option<IFileLeasedAccess>> AcquireLease(TimeSpan leaseDuration, ScopeContext context) => _fileClient.AcquireLease(_datalakeStore, leaseDuration, context);
    public Task<Option<IFileLeasedAccess>> AcquireExclusiveLease(bool breakLeaseIfExist, ScopeContext context) => _fileClient.AcquireExclusiveLease(_datalakeStore, breakLeaseIfExist, context);
    public Task<Option> BreakLease(ScopeContext context) => DatalakeLeaseTool.Break(_fileClient, context);
}
