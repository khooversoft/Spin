using System.Collections.Frozen;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class PathTool
{
    private static FrozenSet<(string chr, string replace)> _replaceMap = new[]
    {
        ( "/", "___" ),
        ( ":", "__" ),
        ( "$", "_DLR_" ),
    }.ToFrozenSet();

    private static string ToEncoding(string value) => _replaceMap.Aggregate(value, (x, y) => x.Replace(y.chr, y.replace));
    private static string ToDecoding(string value) => _replaceMap.Aggregate(value, (x, y) => x.Replace(y.replace, y.chr));

    public static string ToExtension(string extension) => extension.NotEmpty().StartsWith(".") ? extension : "." + extension;

    public static string SetExtension(string path, string? extension)
    {
        if (extension.IsEmpty()) return path;

        path.NotEmpty();
        extension = ToExtension(extension);

        var dotCount = extension.WithIndex().Where(x => x.Item == '.').Count();

        var indexInMessage = path
            .Reverse().WithIndex()
            .TakeWhile(x => x.Item != '\\' || x.Item != ':')
            .Where(x => x.Item == '.')
            .Select(x => x.Index)
            .Take(dotCount)
            .ToArray();

        if (indexInMessage.Length != dotCount)
        {
            return simpleForm();
        }

        int trimIndex = indexInMessage.Last() switch
        {
            0 => path.Length - 1,
            int v => path.Length - v - 1,
        };

        return path[0..trimIndex] + extension;

        string simpleForm() => path.LastIndexOf('.') switch
        {
            -1 => path += extension,
            var v => path[0..v] + extension,
        };
    }

    public static string RemoveExtension(string path, string extension, params string[] extensions)
    {
        path.NotEmpty();

        var extensionList = extension.ToEnumerable()
            .Concat(extensions)
            .Select(x => ToExtension(x))
            .ToArray();

        foreach (var item in extensionList)
        {
            if (path.EndsWith(item)) return path[0..^(item.Length)];
        }

        return path;
    }

    //public static string CreateFileId(string path, string? extension = null)
    //{
    //    string[] parts = path.NotEmpty().Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();
    //    if (parts.Length < 2) return PathTool.SetExtension(path, extension);

    //    string f1 = PathTool.SetExtension(parts[^1], extension);
    //    string newPath = parts[..^1].Append(f1).Join('/');
    //    string encodedPath = ToEncoding(newPath);

    //    string storePath = parts[..^1]
    //        .Select(x => ToEncoding(x))
    //        .Append(encodedPath)
    //        .Join('/');

    //    return storePath.ToLower();
    //}
}