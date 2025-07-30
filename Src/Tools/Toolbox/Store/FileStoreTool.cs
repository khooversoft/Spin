using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public static class FileStoreTool
{
    public static bool IsPathValid(string path) => PathValidator.IsPathValid(path);

    public static string? GetLeaseId(this IFileReadWriteAccess subject) => subject switch
    {
        IFileLeasedAccess v => v.LeaseId,
        _ => null
    };

    public static async Task<Option> ClearStore<T>(this IHost host)
    {
        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var context = host.Services.CreateContext<T>();
        var result = await fileStore.ClearStore(context);
        return result;
    }

    public static async Task<Option> ClearStore(this IFileStore subject, ScopeContext context)
    {
        using var metric = context.LogDuration("fileStore-clear", "store={store}", subject.GetType().Name);

        context.LogDebug("Clearing file store {store}", subject.GetType().Name);
        IReadOnlyList<IStorePathDetail> pathItems = await subject.Search("*;includeFolder=true", context).ConfigureAwait(false);
        pathItems.NotNull();

        foreach (var item in pathItems)
        {
            switch (item.IsFolder)
            {
                case true:
                    var deleteFolderOption = await subject.DeleteFolder(item.Path, context).ConfigureAwait(false);
                    if (deleteFolderOption.IsError()) return deleteFolderOption;
                    break;
                case false:
                    var deleteOption = await subject.File(item.Path).Delete(context).ConfigureAwait(false);
                    if (deleteOption.IsError()) return deleteOption;
                    break;
            }
        }

        return StatusCode.OK;
    }
}