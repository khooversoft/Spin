﻿using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueAdmin
    {
        private readonly ServiceBusAdministrationClient _managementClient;
        private readonly ILogger<QueueAdmin> _logging;

        public QueueAdmin(QueueOption queueOption, ILogger<QueueAdmin> logging)
        {
            queueOption.NotNull();
            logging.NotNull();

            ConnectionString = queueOption.ToConnectionString();
            _managementClient = new ServiceBusAdministrationClient(ConnectionString);
            _logging = logging;
        }

        public string ConnectionString { get; }

        public async Task<bool> Exist(string queueName, CancellationToken token = default)
        {
            queueName.NotEmpty();

            bool exist = await _managementClient.QueueExistsAsync(queueName, token);
            _logging.LogTrace($"{nameof(Exist)}: Queue={queueName}, return={exist}");
            return exist;
        }

        public async Task<QueueDefinition> Create(QueueDefinition queueDefinition, CancellationToken token = default)
        {
            queueDefinition.NotNull();

            QueueProperties createdDescription = await _managementClient.CreateQueueAsync(queueDefinition.ToCreateQueue(), token);
            _logging.LogTrace($"{nameof(Create)}: QueueName={queueDefinition.QueueName}");

            return createdDescription.ConvertTo();
        }

        public async Task<QueueDefinition> GetDefinition(string queueName, CancellationToken token = default)
        {
            queueName.NotEmpty();

            QueueProperties queueDescription = await _managementClient.GetQueueAsync(queueName, token);
            _logging.LogTrace($"{nameof(GetDefinition)}: QueueName={queueName}");
            return queueDescription.ConvertTo();
        }

        public async Task Delete(string queueName, CancellationToken token = default)
        {
            queueName.NotEmpty();

            _logging.LogTrace($"{nameof(Delete)}: QueueName={queueName}");
            await _managementClient.DeleteQueueAsync(queueName, token);
        }

        public async Task<QueueDefinition> CreateIfNotExist(QueueDefinition queueDefinition, CancellationToken token = default)
        {
            if ((await Exist(queueDefinition.QueueName, token))) return queueDefinition;

            return await Create(queueDefinition, token);
        }

        public async Task DeleteIfExist(string queueName, CancellationToken token = default)
        {
            queueName.NotEmpty();

            bool exist = await Exist(queueName, token);
            if (exist)
            {
                await Delete(queueName, token);
            }
        }
    }
}
