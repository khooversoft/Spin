using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public static class KeyStoreTool
{
    public static bool IsPathValid(string path) => PathValidator.IsPathValid(path);

    public static string? GetLeaseId(this IFileReadWriteAccess subject) => subject switch
    {
        IFileLeasedAccess v => v.LeaseId,
        _ => null
    };

    public static async Task<Option> ClearKeyStore<T>(this IHost host)
    {
        IKeyStore fileStore = host.Services.GetRequiredService<IKeyStore>();
        var context = host.Services.CreateContext<T>();
        var result = await fileStore.ClearStore(context);
        return result;
    }

    public static Task<Option> ClearStore(this IKeyStore subject, ScopeContext context) => ClearFolder(subject, null, context);

    public static async Task<Option> ClearFolder(this IKeyStore fileStore, string? path, ScopeContext context)
    {
        using var metric = context.LogDuration("fileStore-clear", "store={store}", fileStore.GetType().Name);
        context.LogDebug("Clearing file store path={path}", path);

        string pattern = $"{buildPattern()};includeFolder=true";

        IReadOnlyList<StorePathDetail> pathItems = await fileStore.Search(pattern, context);
        var deleteFolderOption = await InternalDelete(fileStore, pathItems, context);

        return StatusCode.OK;

        string buildPattern() => path switch
        {
            null => "*",
            string => fixPath(path),
        };

        static string fixPath(string pathToFix)
        {
            pathToFix.Contains('*').BeFalse("Path should not contain wildcard characters");

            return pathToFix.EndsWith('/') switch
            {
                true => pathToFix + "**/*",
                false => pathToFix + "/**/*"
            };
        }
    }

    private static async Task<Option> InternalDelete(IKeyStore fileStore, IReadOnlyList<StorePathDetail> pathItems, ScopeContext context)
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
                    var deleteOption = await fileStore.Delete(item.Path, context).ConfigureAwait(false);
                    if (deleteOption.IsError()) return deleteOption.LogStatus(context, "Failed to delete file");
                    break;
            }
        }

        return StatusCode.OK;
    }
}
