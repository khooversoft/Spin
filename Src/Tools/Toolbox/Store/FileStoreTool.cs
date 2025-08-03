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

    public static Task<Option> ClearStore(this IFileStore subject, ScopeContext context) => ClearFolder(subject, null, context);

    public static async Task<Option> ClearFolder(this IFileStore fileStore, string? path, ScopeContext context)
    {
        using var metric = context.LogDuration("fileStore-clear", "store={store}", fileStore.GetType().Name);
        context.LogDebug("Clearing file store path={path}", path);

        string pattern = $"{buildPattern()};includeFolder=true";

        IReadOnlyList<IStorePathDetail> pathItems = (await fileStore.Search(pattern, context)).Where(x => x.IsFolder).ToArray();
        var deleteFolderOption = await InternalDelete(fileStore, pathItems, context);

        pathItems = (await fileStore.Search(pattern, context));
        deleteFolderOption = await InternalDelete(fileStore, pathItems, context);

        return StatusCode.OK;

        string buildPattern() => path switch
        {
            null => "**/*",
            string => path.EndsWith('/') ? path + "**/*" : path + "/**/*",
        };
    }

    private static async Task<Option> InternalDelete(IFileStore fileStore, IReadOnlyList<IStorePathDetail> pathItems, ScopeContext context)
    {
        foreach (var item in pathItems)
        {
            switch (item.IsFolder)
            {
                case true:
                    var deleteFolderOption = await fileStore.DeleteFolder(item.Path, context).ConfigureAwait(false);
                    if (deleteFolderOption.IsError()) return deleteFolderOption.LogStatus(context, "Failed to delete folder");
                    break;
                case false:
                    var deleteOption = await fileStore.File(item.Path).Delete(context).ConfigureAwait(false);
                    if (deleteOption.IsError()) return deleteOption.LogStatus(context, "Failed to delete file");
                    break;
            }
        }

        return StatusCode.OK;
    }
}