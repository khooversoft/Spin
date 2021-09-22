using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace Toolbox.Configuration
{
    public static class ConfigurationTools
    {
        public static IReadOnlyList<string> GetJsonFiles(string file, IPropertyResolver resolver)
        {
            file.VerifyNotEmpty(nameof(file));
            resolver.VerifyNotNull(nameof(resolver));

            file = resolver.Resolve(file);
            if (!File.Exists(file)) return new[] { file };

            string folder = Path.GetDirectoryName(file)!;

            var stack = new Stack<string>(new[] { file });
            var list = new List<string>();

            while (stack.TryPop(out string? inlcudeFile))
            {
                list.Add(inlcudeFile);
                File.Exists(inlcudeFile).VerifyAssert(x => x == true, $"File {inlcudeFile} does not exist");

                new ConfigurationBuilder()
                    .AddJsonFile(inlcudeFile)
                    .Build()
                    .AsEnumerable()
                    .Where(x => x.Key.StartsWith("$include"))
                    .Select(x => resolver.Resolve(x.Value))
                    .Select(x => Path.Combine(folder, x))
                    .Reverse()
                    .ForEach(x => stack.Push(x));
            }

            return list;
        }
    }
}