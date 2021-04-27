using System;
using System.IO;
using System.Linq;
using Toolbox.Extensions;

namespace Toolbox.Tools
{
    public static class PathTools
    {
        public static string SetExtension(string path, string extension)
        {
            path.VerifyNotEmpty(nameof(path));
            extension.VerifyNotEmpty(nameof(extension));

            extension = extension.StartsWith(".") ? extension : "." + extension;

            return path.Split('/')
                .Reverse()
                .Select((x, i) => i == 0 ? setExtensionIfRequired(x, extension) : x)
                .Reverse()
                .Aggregate(string.Empty, (a, x) => a += x + "/", x => x[0..^1]);

            static string setExtensionIfRequired(string file, string extension) => Path.GetExtension(file).IsEmpty() ? Path.ChangeExtension(file, extension) : file;
        }
    }
}