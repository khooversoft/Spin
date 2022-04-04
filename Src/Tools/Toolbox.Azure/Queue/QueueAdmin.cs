using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueAdmin
    {
        private readonly ServiceBusAdministrationClient _managementClient;
        private readonly ILogger<QueueAdmin> _logging;

        public QueueAdmin(QueueOption queueOption, ILogger<QueueAdmin> logging)
        {
            queueOption.VerifyNotNull(nameof(queueOption));
            logging.VerifyNotNull(nameof(logging));

            ConnectionString = queueOption.ToConnectionString();
            _managementClient = new ServiceBusAdministrationClient(ConnectionString);
            _logging = logging;
        }

        public string ConnectionString { get; }

        public async Task<bool> Exist(string queueName, CancellationToken token)
        {
            queueName.VerifyNotEmpty(nameof(queueName));

            bool exist = await _managementClient.QueueExistsAsync(queueName, token);
            _logging.LogTrace($"{nameof(Exist)}: Queue={queueName}, return={exist}");
            return exist;
        }

        public async Task<QueueDefinition> Create(QueueDefinition queueDefinition, CancellationToken token)
        {
            queueDefinition.VerifyNotNull(nameof(queueDefinition));

            QueueProperties createdDescription = await _managementClient.CreateQueueAsync(queueDefinition.ToCreateQueue(), token);
            _logging.LogTrace($"{nameof(Create)}: QueueName={queueDefinition.QueueName}");

            return createdDescription.ConvertTo();
        }

        public async Task<QueueDefinition> GetDefinition(string queueName, CancellationToken token)
        {
            queueName.VerifyNotEmpty(nameof(queueName));

            QueueProperties queueDescription = await _managementClient.GetQueueAsync(queueName, token);
            _logging.LogTrace($"{nameof(GetDefinition)}: QueueName={queueName}");
            return queueDescription.ConvertTo();
        }

        public async Task Delete(string queueName, CancellationToken token)
        {
            queueName.VerifyNotEmpty(nameof(queueName));

            _logging.LogTrace($"{nameof(Delete)}: QueueName={queueName}");
            await _managementClient.DeleteQueueAsync(queueName, token);
        }

        public async Task<QueueDefinition> CreateIfNotExist(QueueDefinition queueDefinition, CancellationToken token)
        {
            if ((await Exist(queueDefinition.QueueName, token))) return queueDefinition;

            return await Create(queueDefinition, token);
        }

        public async Task DeleteIfExist(string queueName, CancellationToken token)
        {
            queueName.VerifyNotEmpty(nameof(queueName));

            if ((await Exist(queueName, token))) return;

            await Delete(queueName, token);
        }
    }
}
