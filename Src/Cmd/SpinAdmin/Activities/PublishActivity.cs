//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using MessageNet.sdk.Host.Model;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Toolbox.Azure.DataLake;
//using Toolbox.Azure.DataLake.Model;
//using Toolbox.Azure.Queue;
//using Toolbox.Configuration;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace SpinAdmin.Activities
//{
//    internal class PublishActivity
//    {
//        private readonly ConfigurationStore _configurationStore;
//        private readonly ILoggerFactory _loggerFactory;
//        private readonly ILogger<PublishActivity> _logger;

//        public PublishActivity(ConfigurationStore configurationStore, ILoggerFactory loggerFactory)
//        {
//            configurationStore.VerifyNotNull(nameof(configurationStore));
//            loggerFactory.VerifyNotNull(nameof(loggerFactory));

//            _configurationStore = configurationStore;
//            _loggerFactory = loggerFactory;
//            _logger = _loggerFactory.CreateLogger<PublishActivity>();
//        }

//        public async Task Publish(string store, string environment, CancellationToken token)
//        {
//            _logger.LogInformation($"{nameof(Publish)}: Publishing configuration");

//            string configurationFile = _configurationStore
//                .Environment(store, environment)
//                .File
//                .GetConfigurationFile();

//            var dict = new Dictionary<string, string>()
//            {
//                ["environment"] = environment
//            };

//            EnvironmentModel model = new ConfigurationBuilder()
//                .AddInMemoryCollection(dict)
//                .AddJsonFile(configurationFile, JsonFileOption.Enhance)
//                .AddPropertyResolver()
//                .Build()
//                .Bind<EnvironmentModel>();

//            if (model == null)
//            {
//                _logger.LogError($"{nameof(Publish)}: No configuration file found");
//                return;
//            }

//            await SetStorage(model, token);
//            await SetQueue(model, token);
//        }

//        private async Task SetStorage(EnvironmentModel model, CancellationToken token)
//        {
//            _logger.LogInformation($"{nameof(SetStorage)}: Publishing to storage accounts");

//            foreach (StorageRecord storage in model.Storage)
//            {
//                DataLakeStoreOption option = new()
//                {
//                    AccountName = storage.AccountName,
//                    ContainerName = storage.ContainerName,
//                    AccountKey = storage.AccountKey,
//                };

//                IDataLakeFileSystem management = new DataLakeFileSystem(option, _loggerFactory.CreateLogger<DataLakeFileSystem>());
//                await management.CreateIfNotExist(storage.ContainerName, token);

//                _logger.LogInformation($"{nameof(SetStorage)}: Set container {storage.ContainerName} on storage {storage.AccountName}");
//            }
//        }

//        private async Task SetQueue(EnvironmentModel model, CancellationToken token)
//        {
//            _logger.LogInformation($"{nameof(SetStorage)}: Publishing to storage accounts");

//            foreach (QueueRecord queue in model.Queue)
//            {
//                queue.AuthManage.VerifyNotEmpty($"authManage is required");

//                (string KeyName, string AccessKey) = QueueAuthorization.Parse(queue.AuthManage);

//                QueueOption option = new()
//                {
//                    Namespace = queue.Namespace,
//                    QueueName = queue.QueueName,
//                    KeyName = KeyName,
//                    AccessKey = AccessKey,
//                };

//                var admin = new QueueAdmin(option, _loggerFactory.CreateLogger<QueueAdmin>());

//                var defintion = new QueueDefinition()
//                {
//                    QueueName = queue.QueueName,
//                };

//                await admin.CreateIfNotExist(defintion, token);

//                _logger.LogInformation($"{nameof(SetQueue)}: Set {queue.Channel} on queue {queue.QueueName} on Service Bus namespace {queue.Namespace}");
//            }
//        }
//    }
//}