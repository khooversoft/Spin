using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Extensions;
using Toolbox.Types;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Store;

namespace Toolbox.Azure;

public static class DatalakeTool
{
    public static async Task<Option<IStorePathDetail>> GetPathDetail(this DataLakeFileClient fileClient, ScopeContext context)
    {
        fileClient.NotNull();

        context.Location().LogTrace("Getting path {path} properties", fileClient.Path);
        using var metric = context.LogDuration("dataLakeStore-getPathProperties");

        try
        {
            Response<bool> exist = await fileClient.ExistsAsync();
            if (!exist.HasValue || !exist.Value)
            {
                context.Location().LogTrace("File does not exist, path={path}", fileClient.Path);
                return new Option<IStorePathDetail>(StatusCode.NotFound);
            }

            var result = await fileClient.GetPropertiesAsync(cancellationToken: context).ConfigureAwait(false);
            return result.Value.ConvertTo(fileClient.Path).ToOption();
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to GetPathProperties for file {path}", fileClient.Path);
            return (StatusCode.NotFound, ex.Message);
        }
    }

    public static async Task<Option<IStorePathDetail>> GetPathDetailOrCreate(this DataLakeFileClient fileClient, ScopeContext context)
    {
        using var metric = context.LogDuration("dataLakeStore-getPathPropertiesOrCreate");

        var properties = await fileClient.GetPathDetail(context).ConfigureAwait(false);
        if (properties.IsOk()) return properties;

        await fileClient.CreateIfNotExistsAsync(PathResourceType.File);
        return await fileClient.GetPathDetail(context).ConfigureAwait(false);
    }

    public static IStorePathDetail ConvertTo(this PathItem subject, string path)
    {
        subject.NotNull();
        path.NotEmpty();

        return new StorePathDetail
        {
            Path = path,
            IsFolder = subject.IsDirectory ?? false,
            LastModified = subject.LastModified,
            ContentLength = subject.ContentLength ?? 0,
            CreatedOn = subject.CreatedOn,
            ETag = subject.ETag.ToString(),
        };
    }

    public static IStorePathDetail ConvertTo(this PathProperties subject, string path)
    {
        subject.NotNull();
        path.NotEmpty();

        return new StorePathDetail
        {
            Path = path,
            IsFolder = false,
            LastModified = subject.LastModified,
            ContentLength = subject.ContentLength,
            CreatedOn = subject.CreatedOn,
            ETag = subject.ETag.ToString(),
        };
    }
}
