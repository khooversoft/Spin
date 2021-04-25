using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Toolbox.Tools.Property
{
    public class PropertySecret
    {
        public PropertySecret(string secretId, IEnumerable<KeyValuePair<string, string>> properties)
        {
            VerifySecretId(secretId);
            properties.VerifyNotNull(nameof(properties));

            SecretId = secretId;
            Properties = properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        public PropertySecret(IEnumerable<KeyValuePair<string, string>> properties)
        {
            properties.VerifyNotNull(nameof(properties));

            Properties = properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        public string? SecretId { get; }

        public IDictionary<string, string> Properties { get; }

        public void WriteToSecret(string? secretId = null)
        {
            secretId = VerifySecretId(secretId ?? SecretId);

            string file = GetSecretFilePath(secretId);

            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, Json.Default.SerializeFormat(Properties));
        }

        public static PropertySecret ReadFromSecret(string secretId, bool optional = false)
        {
            VerifySecretId(secretId);

            Dictionary<string, string>? properties = null;

            string file = GetSecretFilePath(secretId);

            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                properties = Json.Default.Deserialize<Dictionary<string, string>>(json).VerifyNotNull($"Cannot deserialize database file={file}");
            }
            else
            {
                if (!optional) throw new FileNotFoundException(file);
            }

            return new PropertySecret(secretId, properties ?? new Dictionary<string, string>());
        }

        private static string GetSecretFilePath(string secretId) => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Microsoft\\UserSecrets\\{secretId}\\property.json";

        private static string VerifySecretId(string? secretId) => secretId
            .VerifyNotEmpty($"{nameof(secretId)} is required")
            .VerifyAssert(x => x.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-'), x => $"{x} is invalid.  Secret id is alpha numeric, or '-', '.'");
    }
}