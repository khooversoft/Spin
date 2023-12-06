using Microsoft.Extensions.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Configuration
{
    public static class ConfigurationTools
    {
        /// <summary>
        /// Return list of json files by searching for "$include*" instructions
        /// </summary>
        /// <param name="file">root json file (start search)</param>
        /// <param name="resolver">Property resolver</param>
        /// <returns>list of Json files</returns>
        public static IReadOnlyList<string> GetJsonFiles(string file, IPropertyResolver resolver)
        {
            file.NotEmpty();
            resolver.NotNull();

            file = resolver.Resolve(file);
            if (!File.Exists(file)) return new[] { file };

            string folder = Path.GetDirectoryName(file)!;

            var stack = new Stack<string>(new[] { file });
            var list = new List<string>();

            while (stack.TryPop(out string? inlcudeFile))
            {
                list.Add(inlcudeFile);
                File.Exists(inlcudeFile).Assert(x => x == true, $"File {inlcudeFile} does not exist");

                try
                {
                    new ConfigurationBuilder()
                        .AddJsonFile(inlcudeFile)
                        .Build()
                        .AsEnumerable()
                        .Where(x => x.Key.StartsWith("$include"))
                        .Select(x => resolver.Resolve(x.Value))
                        .OfType<string>()
                        .Select(x => Path.Combine(folder, x))
                        .Reverse()
                        .ForEach(x => stack.Push(x));
                }
                catch (InvalidDataException) { }
            }

            return list;
        }
    }
}