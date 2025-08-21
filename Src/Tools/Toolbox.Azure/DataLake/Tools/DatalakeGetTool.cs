using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class DatalakeGetTool
{
    public static async Task<Option<DataETag>> Get(this DataLakeFileClient fileClient, ScopeContext context)
    {
        context.LogDebug("Getting file {path} without lease", fileClient.Path);
        var result = await InternalRead(fileClient, null, context);
        return result;
    }

    public static Task<Option<DataETag>> Get(this DataLakeFileClient fileClient, DatalakeLeasedAccess datalakeLease, ScopeContext context)
    {
        context.LogDebug("Getting file path={path}, leaseId={leaseId}", fileClient.Path, datalakeLease.LeaseId);
        var readOption = new DataLakeFileReadOptions
        {
            Conditions = new DataLakeRequestConditions { LeaseId = datalakeLease.LeaseId }
        };

        return InternalRead(fileClient, readOption, context);
    }

    private static async Task<Option<DataETag>> InternalRead(DataLakeFileClient fileClient, DataLakeFileReadOptions? options, ScopeContext context)
    {
        fileClient.NotNull();

        using var metric = context.LogDuration("dataLakeStore-read", "path={path}", fileClient.Path);
        context.LogDebug("Reading file {path}, isOptions={isOptions}, leaseId?={leaseId}", fileClient.Path, (options != null).ToString(), options?.Conditions.LeaseId ?? "<no data>");

        try
        {
            var ifExists = await fileClient.ExistsAsync(context);
            metric.Log("InternalRead");
            if (ifExists.Value == false)
            {
                context.LogDebug("File not found, path={path}", fileClient.Path);
                return StatusCode.NotFound;
            }

            Response<FileDownloadInfo> response = options switch
            {
                null => await fileClient.ReadAsync(context.CancellationToken),
                var v => await fileClient.ReadAsync(v, context.CancellationToken),
            };

            if (response.Value == null) return StatusCode.NotFound;
            metric.Log("readAsync");

            using MemoryStream memory = new MemoryStream();
            await response.Value.Content.CopyToAsync(memory);

            byte[] data = memory.ToArray();
            string etag = response.Value.Properties.ETag.ToString();
            context.LogDebug("Read file {path}, size={size}, eTag={etag}", fileClient.Path, data.Length, etag);
            return new DataETag(data, etag);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            context.LogDebug("File not found {path}", fileClient.Path);
            return (StatusCode.NotFound, $"File not found, path={fileClient.Path}");
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to read file path={path}, leaseId?={leaseId}", fileClient.Path, options?.Conditions.LeaseId ?? "<no data>");
            return (StatusCode.BadRequest, ex.ToString());
        }
    }
}
