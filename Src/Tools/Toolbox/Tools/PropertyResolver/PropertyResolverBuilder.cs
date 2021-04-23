using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Tools.PropertyResolver
{
    public class PropertyResolverBuilder : IPropertyResolverBuilder
    {
        private Database _database = new Database();

        public static string Extension { get; } = ".jsonDb";

        public string this[string key] { get => _database.Data[key]; set => _database.Data[key] = value; }

        public void Build(string file)
        {
            file.VerifyNotEmpty(nameof(file));

            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, Json.Default.SerializeFormat(_database));
        }

        public void BuildForSecretId(string secretId)
        {
            secretId.VerifyNotEmpty(nameof(secretId));

            Build(GetDatabasePath(secretId));
        }

        public IReadOnlyList<KeyValuePair<string, string>> List() => _database.Data.ToList();

        public PropertyResolverBuilder LoadFromFile(string file, bool optional = false)
        {
            file.VerifyNotEmpty(nameof(file));

            if (File.Exists(file))
            {
                if (!optional) throw new FileNotFoundException(file);

                string json = File.ReadAllText(file);
                _database = Json.Default.Deserialize<Database>(json).VerifyNotNull($"Cannot deserialize database file={file}");
            }

            return this;
        }

        public PropertyResolverBuilder LoadFromSecretId(string secretId)
        {
            secretId.VerifyNotEmpty(nameof(secretId));

            string path = GetDatabasePath(secretId);

            string json = File.ReadAllText(path);
            _database = Json.Default.Deserialize<Database>(json).VerifyNotNull("Cannot deserialize database");

            return this;
        }

        public bool Remove(string key) => _database.Data.Remove(key);

        public void Set(string key, string value)
        {
            key.VerifyNotEmpty(nameof(key));
            value.VerifyNotEmpty(nameof(value));

            _database.Data[key] = value;
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _database.Data.TryGetValue(key, out value);

        private static string GetDatabasePath(string secretId) => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Microsoft\\UserSecrets\\{secretId}\\database.json";

        private record Database
        {
            public IDictionary<string, string> Data { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
