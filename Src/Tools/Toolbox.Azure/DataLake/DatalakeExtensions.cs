using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class DatalakeExtensions
{
    public static async Task<Option> ForceDelete(this IKeyStore keyStore, string path, ILogger logger)
    {
        keyStore.NotNull();
        path.NotEmpty();

        var deleteOption = (await keyStore.Delete(path)).LogStatus(logger, "Delete file {path}", [path]);
        if (deleteOption.IsOk() || !deleteOption.IsLocked()) return StatusCode.OK;

        (await keyStore.BreakLease(path)).LogStatus(logger, "Break lease {path}", [path]);

        var result = (await keyStore.Delete(path)).LogStatus(logger, "Delete file {path}", [path]);
        return result;
    }

    public static async Task<Option<string>> ForceSet(this IKeyStore keyStore, string path, DataETag data, ILogger logger)
    {
        keyStore.NotNull();
        path.NotEmpty();

        var writeOption = (await keyStore.Set(path, data)).LogStatus(logger, "Set file {path}", [path]);
        if (writeOption.IsOk() || !writeOption.IsLocked()) return StatusCode.OK;

        (await keyStore.BreakLease(path)).LogStatus(logger, "Break lease {path}", path);

        var result = (await keyStore.Set(path, data)).LogStatus(logger, "Set file {path}", path);
        return result;
    }

    public static async Task<Option<StorePathDetail>> GetPathDetail(this DataLakeFileClient fileClient, ILogger logger, CancellationToken token = default)
    {
        fileClient.NotNull();
        logger.NotNull();

        logger.LogDebug("Getting path {path} properties", fileClient.Path);
        using var metric = logger.LogDuration("dataLakeStore-getPathProperties");

        try
        {
            Response<bool> exist = await fileClient.ExistsAsync();
            if (!exist.HasValue || !exist.Value)
            {
                logger.LogDebug("File does not exist, path={path}", fileClient.Path);
                return new Option<StorePathDetail>(StatusCode.NotFound);
            }

            var result = await fileClient.GetPropertiesAsync(cancellationToken: token);
            return result.Value.ConvertTo(fileClient.Path).ToOption();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to GetPathProperties for file {path}", fileClient.Path);
            return (StatusCode.NotFound, ex.Message);
        }
    }

    public static async Task<Option<StorePathDetail>> GetPathDetailOrCreate(this DataLakeFileClient fileClient, ILogger logger, CancellationToken token = default)
    {
        using var metric = logger.LogDuration("dataLakeStore-getPathPropertiesOrCreate");

        var properties = await fileClient.GetPathDetail(logger, token);
        if (properties.IsOk()) return properties;

        await fileClient.CreateIfNotExistsAsync(PathResourceType.File);
        return await fileClient.GetPathDetail(logger, token);
    }

    public static StorePathDetail ConvertTo(this PathItem subject, string path)
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

    public static StorePathDetail ConvertTo(this PathProperties subject, string path)
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
            LeaseStatus = subject.LeaseStatus switch
            {
                DataLakeLeaseStatus.Locked => LeaseStatus.Locked,
                DataLakeLeaseStatus.Unlocked => LeaseStatus.Unlocked,
                _ => LeaseStatus.Unlocked,
            },
            LeaseDuration = subject.LeaseDuration switch
            {
                DataLakeLeaseDuration.Fixed => LeaseDuration.Fixed,
                DataLakeLeaseDuration.Infinite => LeaseDuration.Infinite,
                _ => LeaseDuration.Infinite,
            },
        };
    }
}
