using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Types;
using Toolbox.Logging;
using Toolbox.Tools;

namespace Toolbox.Azure;

public static class DatalakeGetTool
{
    public static Task<Option<DataETag>> Get(this DataLakeFileClient fileClient, ScopeContext context) => InternalRead(fileClient, null, context);

    public static Task<Option<DataETag>> Get(this DataLakeFileClient fileClient, DatalakeLeasedAccess datalakeLease, ScopeContext context)
    {
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

        try
        {
            var ifExists = await fileClient.ExistsAsync(context).ConfigureAwait(false);
            if (ifExists.Value == false) return StatusCode.NotFound;
            metric.Log("existsAsync");

            Response<FileDownloadInfo> response = options switch
            {
                null => await fileClient.ReadAsync(context.CancellationToken).ConfigureAwait(false),
                var v => await fileClient.ReadAsync(v, context.CancellationToken).ConfigureAwait(false),
            };

            if (response.Value == null) return StatusCode.NotFound;
            metric.Log("readAsync");

            using MemoryStream memory = new MemoryStream();
            await response.Value.Content.CopyToAsync(memory).ConfigureAwait(false);

            byte[] data = memory.ToArray();
            string etag = response.Value.Properties.ETag.ToString();
            context.Location().LogTrace("Read file {path}, size={size}, eTag={etag}", fileClient.Path, data.Length, etag);
            return new DataETag(data, etag);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            context.Location().LogTrace("File not found {path}", fileClient.Path);
            return (StatusCode.NotFound, $"File not found, path={fileClient.Path}");
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to read file {path}", fileClient.Path);
            return (StatusCode.BadRequest, ex.ToString());
        }
    }
}
