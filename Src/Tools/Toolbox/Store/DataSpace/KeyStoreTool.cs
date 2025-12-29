using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public static class KeyStoreTool
{
    public static bool IsPathValid(string path) => PathValidator.IsPathValid(path);

    public static Task<Option> ClearStore(this IKeyStore subject) => ClearFolder(subject, null);

    public static async Task<Option> ClearFolder(this IKeyStore fileStore, string? path)
    {
        string pattern = $"{buildPattern()};includeFolder=true";

        IReadOnlyList<StorePathDetail> pathItems = await fileStore.Search(pattern);
        var deleteFolderOption = await InternalDelete(fileStore, pathItems);

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

    private static async Task<Option> InternalDelete(IKeyStore fileStore, IReadOnlyList<StorePathDetail> pathItems)
    {
        foreach (var item in pathItems)
        {
            switch (item.IsFolder)
            {
                case true:
                    var deleteFolderOption = await fileStore.DeleteFolder(item.Path).ConfigureAwait(false);
                    if (deleteFolderOption.IsError()) return deleteFolderOption;
                    break;
                case false:
                    var deleteOption = await fileStore.Delete(item.Path).ConfigureAwait(false);
                    if (deleteOption.IsError()) return deleteOption;
                    break;
            }
        }

        return StatusCode.OK;
    }
}
