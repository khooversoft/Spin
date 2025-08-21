using System.Collections.Concurrent;
using System.Collections.Immutable;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class DatalakePathTool
{
    public static async Task<Option<IStorePathDetail>> GetPathDetail(this DataLakeFileClient fileClient, ScopeContext context)
    {
        fileClient.NotNull();

        context.Location().LogDebug("Getting path {path} properties", fileClient.Path);
        using var metric = context.LogDuration("dataLakeStore-getPathProperties");

        try
        {
            Response<bool> exist = await fileClient.ExistsAsync();
            if (!exist.HasValue || !exist.Value)
            {
                context.LogDebug("File does not exist, path={path}", fileClient.Path);
                return new Option<IStorePathDetail>(StatusCode.NotFound);
            }

            var result = await fileClient.GetPropertiesAsync(cancellationToken: context);
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

        var properties = await fileClient.GetPathDetail(context);
        if (properties.IsOk()) return properties;

        await fileClient.CreateIfNotExistsAsync(PathResourceType.File);
        return await fileClient.GetPathDetail(context);
    }

    public static async Task<Option<IReadOnlyList<IStorePathDetail>>> GetFileHashes(this IFileStore fileStore, IReadOnlyList<IStorePathDetail> subjects, ScopeContext context)
    {
        fileStore.NotNull();
        subjects.NotNull();
        ConcurrentQueue<IStorePathDetail> pathDetails = new ConcurrentQueue<IStorePathDetail>();

        await Parallel.ForEachAsync(subjects, context.CancellationToken, async (subject, token) =>
        {
            var contentOption = await fileStore.File(subject.Path).Get(context);
            if (contentOption.IsError())
            {
                contentOption.LogStatus(context, "Failed to get file content for {path}", [subject.Path]);
                return;
            }
            string contentHash = contentOption.Return().Data.ToHexHash();
            var newPathDetail = subject.WithContextHash(contentHash);

            pathDetails.Enqueue(newPathDetail);
        });

        return pathDetails.ToImmutableArray();
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
