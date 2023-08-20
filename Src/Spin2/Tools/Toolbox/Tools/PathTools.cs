using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class PathTools
{
    public static string ToExtension(string extension) => extension.NotEmpty().StartsWith(".") ? extension : "." + extension;

    public static string SetExtension(string path, string extension)
    {
        path.NotEmpty();
        extension = ToExtension(extension);

        return path.LastIndexOf('.') switch
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
}