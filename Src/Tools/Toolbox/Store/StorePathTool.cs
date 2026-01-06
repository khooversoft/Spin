using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

public static class StorePathTool
{
    public static string RemoveForwardSlash(string path) => path.NotEmpty().StartsWith('/') switch
    {
        true => path[1..],
        false => path,
    };

    public static string ToFolderSearch(string path, bool recursive = false) => recursive switch
    {
        false => GetRootPath(path) + "/*",
        true => GetRootPath(path) + "/**",
    };

    public static string AddRecursiveSafe(string path) => path.NotEmpty() switch
    {
        "**" => path,
        "***" => "**",
        string _ when path.IsEmpty() => "**",
        string _ when path.EndsWith("/**") => path,
        string _ when path.EndsWith("/*") => path + "*",
        _ => path + "/**",
    };

    public static string GetRootPath(string path, params string[] additionalPaths)
    {
        path.NotEmpty();
        int idx = path.IndexOf('*');

        var rootPath = idx switch
        {
            -1 => path,
            int v => path[..v].Func(x =>
            {
                int lastSlashIdx = x.LastIndexOf('/');
                return lastSlashIdx switch
                {
                    -1 => string.Empty,
                    var v when v == x.Length - 1 => x[..^1],
                    _ => x[..lastSlashIdx],
                };
            })
        };

        var addParts = additionalPaths.SelectMany(x => x.Split('/', StringSplitOptions.RemoveEmptyEntries));

        var fullPath = rootPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Concat(addParts)
            .Join('/');

        return fullPath.ToLowerInvariant();
    }
}