using Toolbox.Types;

namespace Toolbox.Store;

public static class FileStoreTool
{
    public static bool IsPathValid(string path) => IdPatterns.IsPath(path);
}