﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Toolbox.Tools;

//namespace SpinAdmin.Activities
//{
//    internal class SecretActivity
//    {
//        private readonly ConfigurationStore _configurationStore;
//        private readonly ILogger<QueueActivity> _logger;

//        public SecretActivity(ConfigurationStore configurationStore, ILogger<QueueActivity> logger)
//        {
//            _configurationStore = configurationStore;
//            _logger = logger;
//        }

//        public async Task Set(string store, string environment, string key, string secret, CancellationToken token)
//        {
//            key.VerifyNotEmpty(nameof(key));
//            secret.VerifyNotEmpty(nameof(secret));

//            await _configurationStore
//                .Environment(store, environment)
//                .Secret
//                .Set(key, secret, token);
//        }

//        public async Task Delete(string store, string environment, string key, CancellationToken token)
//        {
//            key.VerifyNotEmpty(nameof(key));

//            await _configurationStore
//                .Environment(store, environment)
//                .Secret
//                .Delete(key, token);
//        }

//        public async Task List(string store, string environment, CancellationToken token)
//        {
//            store.VerifyNotEmpty(nameof(store));
//            environment.VerifyNotEmpty(nameof(environment));

//            IReadOnlyList<KeyValuePair<string, string>> secrets = await _configurationStore
//                .Environment(store, environment)
//                .Secret
//                .List(token);

//            var list = new[]
//            {
//                "Listing secrets configurations",
//                "",
//            }
//            .Concat(secrets.Select(x => $"Key={x.Key}, Value={x.Value}"));

//            _logger.LogInformation($"{nameof(List)}: {string.Join(Environment.NewLine, list)}");
//        }
//    }
//}