//using Directory.sdk;
//using Directory.sdk.Model;
//using Microsoft.Extensions.Logging;
//using System.Threading;
//using System.Threading.Tasks;
//using Toolbox.Azure.DataLake;
//using Toolbox.Azure.DataLake.Model;
//using Toolbox.Azure.Queue;
//using Toolbox.Tools;

//namespace SpinAdmin.Activities
//{
//    internal class PublishActivity
//    {
//        private readonly IDirectoryNameService _directoryNameService;
//        private readonly ILoggerFactory _loggerFactory;
//        private readonly ILogger<PublishActivity> _logger;

//        public PublishActivity(IDirectoryNameService configurationStore, ILoggerFactory loggerFactory)
//        {
//            configurationStore.VerifyNotNull(nameof(configurationStore));
//            loggerFactory.VerifyNotNull(nameof(loggerFactory));

//            _directoryNameService = configurationStore;
//            _loggerFactory = loggerFactory;
//            _logger = _loggerFactory.CreateLogger<PublishActivity>();
//        }

//        public async Task Publish(string store, string environment, CancellationToken token)
//        {
//            _logger.LogInformation($"{nameof(Publish)}: Publishing configuration");

//            Database db = _directoryNameService
//                .Load(store, environment)
//                .SelectDefault(environment);

//            if (db == null)
//            {
//                _logger.LogError($"{nameof(Publish)}: No configuration file found");
//                return;
//            }

//            await SetStorage(db, token);
//            await SetQueue(db, token);
//        }

//        private async Task SetStorage(Database db, CancellationToken token)
//        {
//            _logger.LogInformation($"{nameof(SetStorage)}: Publishing to storage accounts");

//            foreach (StorageRecord storage in db.Storage.Values)
//            {
//                DatalakeStoreOption option = new()
//                {
//                    AccountName = storage.AccountName,
//                    ContainerName = storage.ContainerName,
//                    AccountKey = storage.AccountKey,
//                };

//                IDatalakeFileSystem management = new DatalakeFileSystem(option, _loggerFactory.CreateLogger<DatalakeFileSystem>());
//                await management.CreateIfNotExist(storage.ContainerName, token);

//                _logger.LogInformation($"{nameof(SetStorage)}: Set container {storage.ContainerName} on storage {storage.AccountName}");
//            }
//        }

//        private async Task SetQueue(Database db, CancellationToken token)
//        {
//            _logger.LogInformation($"{nameof(SetStorage)}: Publishing to storage accounts");

//            foreach (QueueRecord queue in db.Queue.Values)
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