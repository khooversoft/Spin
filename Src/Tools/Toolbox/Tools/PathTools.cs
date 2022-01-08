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

            return path.EndsWith(extension) ? path: path + extension;
        }

        public static string RemoveExtension(string path, string extension, params string[] extensions)
        {
            path.VerifyNotEmpty(nameof(path));

            var extensionList = (extension.ToEnumerable().Concat(extensions))
                .Select(x => x.StartsWith(".") ? x : "." + x)
                .ToArray();

            foreach(var item in extensionList)
            {
                if (path.EndsWith(item)) return path[0..^(extension.Length-1)];
            }

            return path;
        }
    }
}