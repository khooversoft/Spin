using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public static class KeyStoreTool
{
    public static bool IsPathValid(string path) => PathValidator.IsPathValid(path);

    public static async Task<Option> ForceDelete(this IKeyStore keyStore, string path)
    {
        keyStore.NotNull();
        path.NotEmpty();

        var deleteOption = await keyStore.Delete(path);
        if (deleteOption.IsOk() || !deleteOption.IsLocked()) return StatusCode.OK;

        await keyStore.BreakLease(path);

        var result = await keyStore.Delete(path);
        return result;
    }

    public static async Task<Option<string>> ForceSet(this IKeyStore keyStore, string path, DataETag data)
    {
        keyStore.NotNull();
        path.NotEmpty();

        var writeOption = await keyStore.Set(path, data);
        if (writeOption.IsOk() || !writeOption.IsLocked()) return StatusCode.OK;

        await keyStore.BreakLease(path);

        var result = await keyStore.Set(path, data);
        return result;
    }
}
