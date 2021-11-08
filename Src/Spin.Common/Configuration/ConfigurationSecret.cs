//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Spin.Common.Configuration.Model;
//using Toolbox.Tools;

//namespace Spin.Common.Configuration
//{
//    public class ConfigurationSecret
//    {
//        private readonly string _configStorePath;
//        private readonly string _environmentName;
//        private readonly ILogger _logger;

//        internal ConfigurationSecret(string configStorePath, string environmentName, ILogger logger)
//        {
//            configStorePath.VerifyNotEmpty(nameof(configStorePath));
//            environmentName.VerifyNotEmpty(nameof(environmentName));
//            logger.VerifyNotNull(nameof(logger));

//            _configStorePath = configStorePath;
//            _environmentName = environmentName;
//            _logger = logger;
//        }

//        public async Task<string?> Get(string key, CancellationToken token)
//        {
//            string fullPath = ConfigurationStore.GetSecretFile(_configStorePath, _environmentName);
//            key.VerifyNotEmpty(nameof(key));

//            SecretRecord model = File.Exists(fullPath)
//                ? Json.Default.Deserialize<SecretRecord>(await File.ReadAllTextAsync(fullPath, token))!
//                : new SecretRecord();

//            return model.Get(key);
//        }

//        public async Task Set(string key, string secret, CancellationToken token)
//        {
//            string fullPath = ConfigurationStore.GetSecretFile(_configStorePath, _environmentName);
//            key.VerifyNotEmpty(nameof(key));
//            secret.VerifyNotEmpty(nameof(secret));

//            SecretRecord model = File.Exists(fullPath)
//                ? Json.Default.Deserialize<SecretRecord>(await File.ReadAllTextAsync(fullPath, token))!
//                : new SecretRecord();

//            model = model.SetWith(key, secret);

//            await File.WriteAllTextAsync(fullPath, Json.Default.SerializeFormat(model), token);

//            _logger.LogTrace($"{nameof(Set)}: Writing secret for {key}");
//        }

//        public async Task Delete(string key, CancellationToken token)
//        {
//            string fullPath = ConfigurationStore.GetSecretFile(_configStorePath, _environmentName);
//            key.VerifyNotEmpty(nameof(key));

//            File.Exists(fullPath).VerifyAssert(x => true, $"Secret file {fullPath} does not exist", _logger);

//            SecretRecord model = Json.Default.Deserialize<SecretRecord>(await File.ReadAllTextAsync(fullPath, token))!;

//            model = model.DeleteWith(key);

//            await File.WriteAllTextAsync(fullPath, Json.Default.Serialize(model), token);

//            _logger.LogTrace($"{nameof(Delete)}: Delete secret for {key}");
//        }

//        public async Task<IReadOnlyList<KeyValuePair<string, string>>> List(CancellationToken token)
//        {
//            string fullPath = ConfigurationStore.GetSecretFile(_configStorePath, _environmentName);

//            File.Exists(fullPath).VerifyAssert(x => true, $"Secret file {fullPath} does not exist", _logger);

//            SecretRecord model = Json.Default.Deserialize<SecretRecord>(await File.ReadAllTextAsync(fullPath, token))!;

//            return model.GetData()
//                .ToArray();
//        }
//    }
//}