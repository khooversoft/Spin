using Toolbox.Types;

namespace Toolbox.Store;

public static class FileStoreTool
{
    public static bool IsPathValid(string path) => IdPatterns.IsPath(path);

    public static string? GetLeaseId(this IFileReadWriteAccess subject) => subject switch
    {
        IFileLeasedAccess v => v.LeaseId,
        _ => null
    };
}