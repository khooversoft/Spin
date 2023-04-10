using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Extensions;

namespace Toolbox.Tools.Property
{
    public class PropertyFile
    {
        public PropertyFile(string file, IEnumerable<KeyValuePair<string, string>> properties)
        {
            properties.NotNull();

            File = SetExtension(file);
            Properties = properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        public PropertyFile(IEnumerable<KeyValuePair<string, string>> properties)
        {
            properties.NotNull();

            Properties = properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        public string? File { get; }

        public IDictionary<string, string> Properties { get; }

        public void WriteToFile(string? file = null)
        {
            file = SetExtension(file ?? File);

            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            System.IO.File.WriteAllText(file, Json.Default.SerializeFormat(Properties));
        }

        public static PropertyFile ReadFromFile(string file, bool optional = false)
        {
            file = SetExtension(file);

            Dictionary<string, string>? properties = null;

            if (System.IO.File.Exists(file))
            {
                if (!optional) throw new FileNotFoundException(file);

                string json = System.IO.File.ReadAllText(file);
                properties = Json.Default.Deserialize<Dictionary<string, string>>(json).NotNull(name: $"Cannot deserialize database file={file}");
            }

            return new PropertyFile(file, properties ?? new Dictionary<string, string>());
        }

        private static string SetExtension(string? file)
        {
            file.NotEmpty(name: $"{nameof(file)} is required");

            if (!Path.GetExtension(file).IsEmpty()) return file;
            return Path.ChangeExtension(file, ".jsonDb");
        }
    }
}