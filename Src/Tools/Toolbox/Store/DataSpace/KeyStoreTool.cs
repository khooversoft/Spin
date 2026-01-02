using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public static class KeyStoreTool
{
    public static bool IsPathValid(string path) => PathValidator.IsPathValid(path);

    public static async Task<Option<string>> Add<T>(this IKeyStore keyStore, string key, T value)
    {
        var data = value.ToDataETag();
        return await keyStore.NotNull().Add(key, data);
    }

    public static async Task<Option<string>> Set<T>(this IKeyStore keyStore, string key, T value)
    {
        var data = value.ToDataETag();
        return await keyStore.NotNull().Set(key, data);
    }

    public static Task<Option<T>> Get<T>(this IKeyStore keyStore, string key) => Get<T>(keyStore, key, data => data.ToObject<T>());

    public static async Task<Option<T>> Get<T>(this IKeyStore keyStore, string key, Func<DataETag, Option<T>> converter)
    {
        keyStore.NotNull();
        converter.NotNull();

        var getOption = await keyStore.Get(key);
        if (getOption.IsError()) return getOption.ToOptionStatus<T>();

        DataETag data = getOption.Return();
        return converter(data);
    }

    public static Task<Option> ClearStore(this IKeyStore subject) => ClearFolder(subject, "/");

    public static async Task<Option> ClearFolder(this IKeyStore fileStore, string path)
    {
        string pattern = $"{buildPattern()};includeFolder=true";

        IReadOnlyList<StorePathDetail> pathItems = await fileStore.Search(pattern);
        var deleteFolderOption = await InternalDelete(fileStore, pathItems);

        await Task.Delay(TimeSpan.FromMilliseconds(500));
        return StatusCode.OK;

        string buildPattern() => path switch
        {
            null => "*",
            "/" => "*",
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
