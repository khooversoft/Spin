using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class PathTool
{
    public static string ToExtension(string extension) => extension.NotEmpty()[0] == '.' ? extension : "." + extension;

    public static string SetExtension(string path, string? extension)
    {
        if (extension.IsEmpty()) return path;

        path.NotEmpty();
        extension = ToExtension(extension);

        var dotCount = extension.Count(x => x == '.');

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

    public static string CreateHashPath(string fileName, int depth = 2, int width = 2)
    {
        fileName.NotEmpty();
        depth.Assert(x => x > 0, "Depth must be greater than 0");
        width.Assert(x => x > 0, "Width must be greater than 0");
        (depth * width).Assert(x => x <= 64, "Depth * Width must be less than or equal to 64");

        byte[] hashBytes = fileName.NotEmpty().ToLower().ToBytes().ToHash();
        string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        var span = hash.AsSpan();
        int stackSize = (depth * (width + 1)) + depth;
        Span<char> pathSpan = stackalloc char[stackSize];
        int bufferIndex = 0;

        for (int i = 0; i < depth; i++)
        {
            if (bufferIndex > 0) pathSpan[bufferIndex++] = '/';

            span.Slice(i * width, width).CopyTo(pathSpan.Slice(bufferIndex));
            bufferIndex += width;
        }

        var hashPath = new string(pathSpan.Slice(0, bufferIndex));
        return hashPath;
    }
}