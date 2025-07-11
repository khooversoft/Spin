using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class DatalakeExtensions
{
    public static async Task<Option> ForceDelete(this IFileAccess fileAccess, ScopeContext context)
    {
        fileAccess.NotNull();

        var deleteOption = (await fileAccess.Delete(context)).LogStatus(context, "Delete file {path}", [fileAccess.Path]);
        if (deleteOption.IsOk() || !deleteOption.IsLocked()) return StatusCode.OK;

        (await fileAccess.BreakLease(context)).LogStatus(context, "Break lease {path}", [fileAccess.Path]);

        var result = (await fileAccess.Delete(context)).LogStatus(context, "Delete file {path}", [fileAccess.Path]);
        return result;
    }

    public static async Task<Option<string>> ForceSet(this IFileAccess fileAccess, DataETag data, ScopeContext context)
    {
        var writeOption = (await fileAccess.Set(data, context)).LogStatus(context, "Set file {path}", [fileAccess.Path]);
        if (writeOption.IsOk() || !writeOption.IsLocked()) return StatusCode.OK;

        (await fileAccess.BreakLease(context)).LogStatus(context, "Break lease {path}", [fileAccess.Path]);

        var result = (await fileAccess.Set(data, context)).LogStatus(context, "Set file {path}", [fileAccess.Path]);
        return result;
    }
}
