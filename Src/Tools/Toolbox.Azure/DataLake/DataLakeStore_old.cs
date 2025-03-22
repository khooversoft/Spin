//using Azure;
//using Azure.Storage;
//using Azure.Storage.Files.DataLake;
//using Azure.Storage.Files.DataLake.Models;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Azure;

//public class DatalakeStore : IDatalakeStore
//{
//    private readonly DataLakeFileSystemClient _fileSystem;
//    private readonly DatalakeOption _azureStoreOption;
//    private readonly ILogger<DatalakeStore> _logger;
//    private readonly DataLakeServiceClient _serviceClient;
//    private readonly DatalakeReadFileClient _readFileClient;
//    private readonly DatalakeWriteFileClient _writeFileClient;

//    public DatalakeStore(DatalakeOption azureStoreOption, ILogger<DatalakeStore> logger)
//    {
//        azureStoreOption.Validate().ThrowOnError();
//        _azureStoreOption = azureStoreOption;
//        _logger = logger.NotNull();

//        _serviceClient = azureStoreOption.CreateDataLakeServiceClient();

//        _fileSystem = _serviceClient.GetFileSystemClient(azureStoreOption.Container);
//        _fileSystem.Exists().Assert(x => x == true, $"Datalake file system does not exist, containerName={azureStoreOption.Container}");

//        _readFileClient = new DatalakeReadFileClient(_fileSystem, azureStoreOption);
//        _writeFileClient = new DatalakeWriteFileClient(_fileSystem, azureStoreOption);
//    }

//    public Task<Option> Append(string path, DataETag data, ScopeContext context) => _writeFileClient.Append(path, data, context);

//    public async Task<Option<IDatalakeLease>> Acquire(string path, TimeSpan leaseDuration, ScopeContext context)
//    {
//        path = _azureStoreOption.WithBasePath(path.NotEmpty());

//        DataLakeFileClient fileClient = _fileSystem.GetFileClient(path);
//        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();
//        DataLakeLease? lease = null;

//        // Acquire the lease
//        while (lease == null && !context.CancellationToken.IsCancellationRequested)
//        {
//            try
//            {
//                lease = await leaseClient.AcquireAsync(leaseDuration);
//                context.LogTrace("Lease acquired. Duration={duration}, leaseId={leaseId}", leaseDuration.ToString(), lease.LeaseId);
//                return new DatalakeLeasedAccess(fileClient, leaseClient, _logger);
//            }
//            catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
//            {
//                context.LogTrace("Lease already present. Retrying...");
//                await Task.Delay(TimeSpan.FromSeconds(1));
//                continue;
//            }
//            catch (Exception ex)
//            {
//                context.LogError(ex, "Failed to acquire lease, {isCancellationRequested}", context.CancellationToken.IsCancellationRequested);
//                return (StatusCode.Conflict, ex.Message);
//            }
//        }

//        context.LogWarning("Failed to acquire lease and stopped trying");
//        return (StatusCode.Conflict, "Canceled");
//    }

//    public async Task<Option> BreakLease(string path, ScopeContext context)
//    {
//        path = _azureStoreOption.WithBasePath(path.NotEmpty());

//        var fileClient = _fileSystem.GetFileClient(path);
//        DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient();

//        try
//        {
//            var breakResult = await leaseClient.BreakAsync();
//            if (!breakResult.HasValue) return (StatusCode.Conflict, "No lease returned");

//            var lease = breakResult.Value;

//            return breakResult.Value ? StatusCode.OK : StatusCode.Conflict;
//        }
//        catch (Exception ex)
//        {
//            context.LogError(ex, "Failed to break lease");
//            return StatusCode.Conflict;
//        }

//        return StatusCode.OK;
//    }

//    public Task<Option<DataETag>> Read(string path, ScopeContext context) => _readFileClient.Read(path, context.With(_logger));

//    public Task<Option<ETag>> Write(string path, DataETag data, bool overwrite, ScopeContext context) => _writeFileClient.Write(path, data, overwrite, context);

//    public async Task<Option> Delete(string path, ScopeContext context)
//    {
//        context = context.With(_logger);
//        using var metric = context.LogDuration("dataLakeStore-delete", "path={path}", path);

//        path = _azureStoreOption.WithBasePath(path);
//        context.Location().LogTrace("Deleting to {path}", path);

//        try
//        {
//            DataLakeFileClient file = _fileSystem.GetFileClient(path);
//            Response<bool> response = await file.DeleteIfExistsAsync(cancellationToken: context).ConfigureAwait(false);

//            if (!response.Value) context.Location().LogTrace("File path={path} does not exist", path);

//            return response.Value ? StatusCode.OK : StatusCode.NotFound;
//        }
//        catch (Exception ex)
//        {
//            context.Location().LogError(ex, "Failed to delete file {path}", path);
//            return StatusCode.BadRequest;
//        }
//    }

//    public async Task<Option> DeleteDirectory(string path, ScopeContext context)
//    {
//        context = context.With(_logger);
//        using var metric = context.LogDuration("dataLakeStore-deleteDirectory", "path={path}", path);

//        path = _azureStoreOption.WithBasePath(path);
//        context.Location().LogTrace("Deleting directory {path}", path);

//        try
//        {
//            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
//            var response = await directoryClient.DeleteAsync(cancellationToken: context).ConfigureAwait(false);
//            if (response.Status != 200) return (StatusCode.Conflict, response.ReasonPhrase);

//            return StatusCode.OK;
//        }
//        catch (Exception ex)
//        {
//            context.Location().LogError(ex, "Failed to delete directory for {path}", path);
//            return StatusCode.BadRequest;
//        }
//    }

//    public async Task<Option> Exist(string path, ScopeContext context)
//    {
//        context = context.With(_logger);
//        using var metric = context.LogDuration("dataLakeStore-exist", "path={path}", path);

//        path = _azureStoreOption.WithBasePath(path);
//        context.Location().LogTrace("Is path {path} exist", path);

//        try
//        {
//            DataLakeFileClient file = _fileSystem.GetFileClient(path);
//            Response<bool> response = await file.ExistsAsync(context).ConfigureAwait(false);
//            return response.Value ? StatusCode.OK : StatusCode.NotFound;
//        }
//        catch (Exception ex)
//        {
//            context.Location().LogError(ex, "Failed to ExistsAsync for {path}", path);
//            throw;
//        }
//    }

//    public Task<Option<DatalakePathProperties>> GetPathProperties(string path, ScopeContext context)
//    {
//        context = context.With(_logger);
//        path = _azureStoreOption.WithBasePath(path);

//        context.Location().LogTrace("Getting path {path} properties", path);
//        return _fileSystem.GetPathProperties(path, context);
//    }


//    public async Task<Option<QueryResponse<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context)
//    {
//        context = context.With(_logger);
//        queryParameter.NotNull();
//        using var metric = context.LogDuration("dataLakeStore-search", "queryParameter={queryParameter}", queryParameter);

//        queryParameter = queryParameter with
//        {
//            Filter = _azureStoreOption.WithBasePath(queryParameter.Filter),
//            BasePath = _azureStoreOption.WithBasePath(queryParameter.BasePath),
//        };
//        context.Location().LogTrace("Searching {queryParameter}", queryParameter);

//        var collection = new List<DatalakePathItem>();
//        var matcher = queryParameter.GetMatcher();

//        int index = -1;
//        try
//        {
//            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(queryParameter.BasePath, queryParameter.Recurse, cancellationToken: context))
//            {
//                if (!matcher.IsMatch(pathItem.Name, pathItem.IsDirectory == true)) continue;

//                index++;
//                if (index < queryParameter.Index) continue;

//                DatalakePathItem datalakePathItem = pathItem.ConvertTo();

//                collection.Add(datalakePathItem);
//                if (collection.Count >= queryParameter.Count) break;
//            }

//            var list = collection
//                .Select(x => x with { Name = _azureStoreOption.RemoveBaseRoot(x.Name) })
//                .ToList();

//            return new QueryResponse<DatalakePathItem>
//            {
//                Query = queryParameter with { Index = index },
//                Items = list,
//                EndOfSearch = list.Count == 0,
//            };
//        }
//        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
//        {
//            return new QueryResponse<DatalakePathItem>
//            {
//                Query = queryParameter,
//                EndOfSearch = false,
//            };
//        }
//        catch (Exception ex)
//        {
//            context.Location().LogWarning(ex, "Failed to search, query={queryParameter}", queryParameter);
//            return (StatusCode.BadRequest, ex.ToString());
//        }
//    }

//    public async Task<Option> TestConnection(ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.Location().LogTrace("Testing connection");

//        try
//        {
//            Response<bool> response = await _fileSystem.ExistsAsync(cancellationToken: context).ConfigureAwait(false);
//            context.Location().LogTrace("Testing exist of file system, exists={fileSystemExist}", response.Value);
//            return response.Value ? StatusCode.OK : StatusCode.ServiceUnavailable;
//        }
//        catch (Exception ex)
//        {
//            context.Location().LogWarning(ex, "Failed exist for file systgem");
//            return StatusCode.ServiceUnavailable;
//        }
//    }
//}