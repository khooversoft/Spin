using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class PathTool
{
    public static string ToExtension(string extension) => extension.NotEmpty().StartsWith(".") ? extension : "." + extension;

    public static string SetExtension(string path, string extension)
    {
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

        if( indexInMessage.Length != dotCount)
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
}