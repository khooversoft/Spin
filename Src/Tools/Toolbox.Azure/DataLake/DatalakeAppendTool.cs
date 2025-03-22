using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Types;
using Toolbox.Logging;
using Toolbox.Tools;
using Azure.Storage.Files.DataLake;
using Azure;
using Toolbox.Store;

namespace Toolbox.Azure;

public static class DatalakeAppendTool
{
    public static Task<Option> Append(this DataLakeFileClient fileClient, DataETag data, ScopeContext context)
    {
        return InternalAppend(fileClient, null, data, context);
    }

    public static Task<Option> Append(this DataLakeFileClient fileClient, DatalakeLeasedAccess datalakeLease, DataETag data, ScopeContext context)
    {
        var option = new DataLakeFileAppendOptions
        {
            LeaseId = datalakeLease.LeaseId,
        };

        return InternalAppend(fileClient, option, data, context);
    }

    private static async Task<Option> InternalAppend(DataLakeFileClient fileClient, DataLakeFileAppendOptions? appendOptions, DataETag data, ScopeContext context)
    {
        data.NotNull().Assert(x => x.Data.Length > 0, $"{nameof(data)} length must be greater then 0, path={fileClient.Path}");
        using var metric = context.LogDuration("dataLakeStore-append", "path={path}, dataSize={dataSize}", fileClient.Path, data.Data.Length);
        context.Location().LogTrace("Appending to {path}, data.Length={data.Length}", fileClient.Path, data.Data.Length);

        using var memoryBuffer = new MemoryStream(data.Data.ToArray());

        try
        {
            Option<IStorePathDetail> pathDetailOption = await fileClient.GetPathDetailOrCreate(context).ConfigureAwait(false);
            if (pathDetailOption.IsError()) return pathDetailOption.ToOptionStatus();
            var pathDetail = pathDetailOption.Return();


            var response = appendOptions switch
            {
                DataLakeFileAppendOptions v => await fileClient.AppendAsync(memoryBuffer, pathDetail.ContentLength, v, cancellationToken: context).ConfigureAwait(false),
                _ => await fileClient.AppendAsync(memoryBuffer, pathDetail.ContentLength, cancellationToken: context).ConfigureAwait(false)
            };

            await fileClient.FlushAsync(pathDetail.ContentLength + data.Data.Length).ConfigureAwait(false);

            context.Location().LogTrace("Appended to path={path}", fileClient.Path);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
        {
            context.Location().LogTrace("Creating path={path}", fileClient.Path);
            //await fileClient.Write(data, true, context).ConfigureAwait(false);

            return StatusCode.OK;
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to append file {path}", fileClient.Path);
            return (StatusCode.BadRequest, $"Failed to append file {fileClient.Path}");
        }

        return StatusCode.OK;
    }
}
