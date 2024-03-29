﻿namespace Toolbox.Tools;

public class PropertySecret
{
    public PropertySecret(string secretId, IEnumerable<KeyValuePair<string, string>> properties)
    {
        VerifySecretId(secretId);
        properties.NotNull();

        SecretId = secretId;
        Properties = properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    public PropertySecret(IEnumerable<KeyValuePair<string, string>> properties)
    {
        properties.NotNull();

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

            properties = Json.Default.Deserialize<Dictionary<string, string>>(json)
                .NotNull(name: $"Cannot deserialize database file={file}");
        }
        else
        {
            if (!optional) throw new FileNotFoundException(file);
        }

        return new PropertySecret(secretId, properties ?? new Dictionary<string, string>());
    }

    private static string GetSecretFilePath(string secretId) => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Microsoft\\UserSecrets\\{secretId}\\property.json";

    private static string VerifySecretId(string? secretId) => secretId
        .NotEmpty(name: $"{nameof(secretId)} is required")
        .Assert(x => x.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-'), x => $"{x} is invalid.  Secret id is alpha numeric, or '-', '.'");
}