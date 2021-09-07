using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Spin.Common.Configuration
{
    public class ConfigurationSecret
    {
        private readonly ILogger _logger;

        public ConfigurationSecret(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Set(string configStorePath, string environmentName, string key, string secret, CancellationToken token)
        {
            string fullPath = ConfigurationStore.GetSecretFile(configStorePath, environmentName);
            key.VerifyNotEmpty(nameof(key));
            secret.VerifyNotEmpty(nameof(secret));

            SecretModel model = File.Exists(fullPath)
                ? Json.Default.Deserialize<SecretModel>(await File.ReadAllTextAsync(fullPath, token))!
                : new SecretModel();

            model = model with
            {
                Data = (((IEnumerable<KeyValuePair<string, string>>)model.Data) ?? Array.Empty<KeyValuePair<string, string>>())
                    .Where(x => !x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .Append(new KeyValuePair<string, string>(key, secret))
                    .ToDictionary(x => x.Key, x => x.Value)
            };

            await File.WriteAllTextAsync(fullPath, Json.Default.SerializeFormat(model), token);

            _logger.LogTrace($"{nameof(Set)}: Writing secret for {key}");
        }

        public async Task Delete(string configStorePath, string environmentName, string key, CancellationToken token)
        {
            string fullPath = ConfigurationStore.GetSecretFile(configStorePath, environmentName);
            key.VerifyNotEmpty(nameof(key));

            SecretModel model = File.Exists(fullPath)
                ? Json.Default.Deserialize<SecretModel>(await File.ReadAllTextAsync(fullPath, token))!
                : new SecretModel();

            model = model with
            {
                Data = (((IEnumerable<KeyValuePair<string, string>>)model.Data) ?? Array.Empty<KeyValuePair<string, string>>())
                    .Where(x => !x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(x => x.Key, x => x.Value)
            };

            await File.WriteAllTextAsync(fullPath, Json.Default.Serialize(model), token);

            _logger.LogTrace($"{nameof(Delete)}: Delete secret for {key}");
        }

        public async Task<IReadOnlyList<KeyValuePair<string, string>>> List(string configStorePath, string environmentName, CancellationToken token)
        {
            string fullPath = ConfigurationStore.GetSecretFile(configStorePath, environmentName);

            SecretModel model = File.Exists(fullPath)
                ? Json.Default.Deserialize<SecretModel>(await File.ReadAllTextAsync(fullPath, token))!
                : new SecretModel();

            return (((IEnumerable<KeyValuePair<string, string>>)model.Data) ?? Array.Empty<KeyValuePair<string, string>>())
                .ToArray();
        }
    }
}