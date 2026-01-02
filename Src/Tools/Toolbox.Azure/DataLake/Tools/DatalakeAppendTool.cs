//using Azure;
//using Azure.Storage.Files.DataLake;
//using Azure.Storage.Files.DataLake.Models;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Azure;

//public static class DatalakeAppendTool
//{
//    public static Task<Option<string>> Append(this DataLakeFileClient fileClient, string? leaseId, DataETag data, ILogger logger, CancellationToken token = default)
//    {
//        var option = leaseId switch
//        {
//            null => null,
//            string v => new DataLakeFileAppendOptions
//            {
//                LeaseId = leaseId,
//            }
//        };

//        return InternalAppend(fileClient, option, data, logger, token);
//    }

//    private static async Task<Option<string>> InternalAppend(DataLakeFileClient fileClient, DataLakeFileAppendOptions? appendOptions, DataETag data, ILogger logger, CancellationToken token)
//    {
//        fileClient.NotNull();
//        logger.NotNull();

//        data.NotNull().Assert(x => x.Data.Length > 0, $"{nameof(data)} length must be greater then 0, path={fileClient.Path}");
//        using var metric = logger.LogDuration("dataLakeStore-append", "path={path}, dataSize={dataSize}", fileClient.Path, data.Data.Length);
//        logger.LogDebug("Appending to {path}, data.Length={data.Length}", fileClient.Path, data.Data.Length);

//        var resultOption = appendOptions?.LeaseId switch
//        {
//            string leaseId when leaseId.IsNotEmpty() => await LeaseAppend(fileClient, leaseId, data, logger, token),
//            _ => await getLeaseAndAppend()
//        };

//        return resultOption;


//        async Task<Option<string>> getLeaseAndAppend()
//        {
//            logger.LogDebug("Acquiring lease for path={path}", fileClient.Path);

//            var leaseOption = await fileClient.InternalAcquireLease(TimeSpan.FromSeconds(30), logger, token);
//            if (leaseOption.IsError())
//            {
//                logger.LogError("Failed to acquire lease for path={path}", fileClient.Path);
//                return leaseOption;
//            }

//            var leaseId = leaseOption.Return();
//            try
//            {
//                return await LeaseAppend(fileClient, leaseId, data, logger, token);
//            }
//            finally
//            {
//                logger.LogDebug("Releasing lease for path={path}", fileClient.Path);
//                DataLakeLeaseClient leaseClient = fileClient.GetDataLakeLeaseClient(leaseId);
//                await leaseClient.ReleaseLease(fileClient.Path, logger, token);
//            }
//        }
//    }

//    private static async Task<Option<string>> LeaseAppend(DataLakeFileClient fileClient, string leaseId, DataETag data, ILogger logger, CancellationToken token)
//    {
//        var fileAppendOption = new DataLakeFileAppendOptions { LeaseId = leaseId };
//        var flushOptions = new DataLakeFileFlushOptions { Conditions = new DataLakeRequestConditions { LeaseId = leaseId } };

//        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

//        try
//        {
//            Option<StorePathDetail> pathDetailOption = await fileClient.GetPathDetailOrCreate(logger, token);
//            if (pathDetailOption.IsError()) return pathDetailOption.ToOptionStatus<string>();
//            var pathDetail = pathDetailOption.Return();

//            logger.LogDebug("Appending to file with options, path={path}, leaseId={leaseId}", fileClient.Path, fileAppendOption.LeaseId);
//            await fileClient.AppendAsync(memoryBuffer, pathDetail.ContentLength, fileAppendOption, cancellationToken: token);

//            Response<PathInfo> resultLock = await fileClient.FlushAsync(pathDetail.ContentLength + data.Data.Length, options: flushOptions);
//            if (!resultLock.HasValue)
//            {
//                logger.LogDebug("Failed to flush data, path={path}", fileClient.Path);
//                return (StatusCode.Conflict, "Failed to flush data");
//            }

//            return resultLock.Value.ETag.ToString();
//        }
//        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
//        {
//            logger.LogDebug("Creating path={path}", fileClient.Path);
//            return await fileClient.Set(data, logger, token);
//        }
//        catch (TaskCanceledException ex)
//        {
//            logger.LogError(ex, "Task canceled for file {path}", fileClient.Path);
//            return (StatusCode.Conflict, "Task canceled");
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Failed to append file {path}", fileClient.Path);
//            return (StatusCode.BadRequest, $"Failed to append file {fileClient.Path}");
//        }
//    }
//}
