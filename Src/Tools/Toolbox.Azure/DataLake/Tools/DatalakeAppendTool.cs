using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class DatalakeAppendTool
{
    public static Task<Option<string>> Append(this DataLakeFileClient fileClient, DataETag data, ScopeContext context)
    {
        return InternalAppend(fileClient, null, data, context);
    }

    public static Task<Option<string>> Append(this DataLakeFileClient fileClient, DatalakeLeasedAccess datalakeLease, DataETag data, ScopeContext context)
    {
        var option = new DataLakeFileAppendOptions
        {
            LeaseId = datalakeLease.LeaseId,
        };

        return InternalAppend(fileClient, option, data, context);
    }

    private static async Task<Option<string>> InternalAppend(DataLakeFileClient fileClient, DataLakeFileAppendOptions? appendOptions, DataETag data, ScopeContext context)
    {
        data.NotNull().Assert(x => x.Data.Length > 0, $"{nameof(data)} length must be greater then 0, path={fileClient.Path}");
        using var metric = context.LogDuration("dataLakeStore-append", "path={path}, dataSize={dataSize}", fileClient.Path, data.Data.Length);
        context.LogDebug("Appending to {path}, data.Length={data.Length}", fileClient.Path, data.Data.Length);

        var resultOption = appendOptions?.LeaseId switch
        {
            string leaseId when leaseId.IsNotEmpty() => await LeaseAppend(fileClient, leaseId, data, context),
            _ => await getLeaseAndAppend()
        };

        return resultOption;


        async Task<Option<string>> getLeaseAndAppend()
        {
            context.LogDebug("Acquiring lease for path={path}", fileClient.Path);

            var leaseOption = await fileClient.InternalAcquireLease(TimeSpan.FromSeconds(30), context);
            if (leaseOption.IsError())
            {
                context.LogError("Failed to acquire lease for path={path}", fileClient.Path);
                return leaseOption.ToOptionStatus<string>();
            }

            var leaseFile = leaseOption.Return();
            try
            {
                return await LeaseAppend(fileClient, leaseFile.LeaseId, data, context);
            }
            finally
            {
                context.LogDebug("Releasing lease for path={path}", fileClient.Path);
                await leaseFile.ReleaseLease(fileClient.Path, context);
            }
        }
    }

    private static async Task<Option<string>> LeaseAppend(DataLakeFileClient fileClient, string leaseId, DataETag data, ScopeContext context)
    {
        var fileAppendOption = new DataLakeFileAppendOptions { LeaseId = leaseId };
        var flushOptions = new DataLakeFileFlushOptions { Conditions = new DataLakeRequestConditions { LeaseId = leaseId } };

        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

        try
        {
            Option<IStorePathDetail> pathDetailOption = await fileClient.GetPathDetailOrCreate(context);
            if (pathDetailOption.IsError()) return pathDetailOption.ToOptionStatus<string>();
            var pathDetail = pathDetailOption.Return();

            context.LogDebug("Appending to file with options, path={path}, leaseId={leaseId}", fileClient.Path, fileAppendOption.LeaseId);
            await fileClient.AppendAsync(memoryBuffer, pathDetail.ContentLength, fileAppendOption, cancellationToken: context);

            Response<PathInfo> resultLock = await fileClient.FlushAsync(pathDetail.ContentLength + data.Data.Length, options: flushOptions);
            if (!resultLock.HasValue)
            {
                context.LogDebug("Failed to flush data, path={path}", fileClient.Path);
                return (StatusCode.Conflict, "Failed to flush data");
            }

            return resultLock.Value.ETag.ToString();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
        {
            context.LogDebug("Creating path={path}", fileClient.Path);
            return await fileClient.Set(data, context);
        }
        catch (TaskCanceledException ex)
        {
            context.Location().LogError(ex, "Task canceled for file {path}", fileClient.Path);
            return (StatusCode.Conflict, "Task canceled");
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to append file {path}", fileClient.Path);
            return (StatusCode.BadRequest, $"Failed to append file {fileClient.Path}");
        }
    }
}
