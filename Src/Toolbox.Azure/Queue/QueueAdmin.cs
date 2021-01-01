using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueAdmin
    {
        private readonly ManagementClient _managementClient;
        private readonly ILogger<QueueAdmin> _logging;

        public QueueAdmin(QueueOption queueOption, ILogger<QueueAdmin> logging)
        {
            queueOption.VerifyNotNull(nameof(queueOption));
            logging.VerifyNotNull(nameof(logging));

            ConnectionString = queueOption.ToConnectionString();
            _managementClient = new ManagementClient(ConnectionString);
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

        public async Task<QueueDefinition> Update(QueueDefinition queueDefinition, CancellationToken token)
        {
            queueDefinition.VerifyNotNull(nameof(queueDefinition));

            QueueDescription result = await _managementClient.UpdateQueueAsync(queueDefinition.ConvertTo(), token);
            _logging.LogTrace($"{nameof(Update)}: QueueName={queueDefinition.QueueName}");

            return result.ConvertTo();
        }

        public async Task<QueueDefinition> Create(QueueDefinition queueDefinition, CancellationToken token)
        {
            queueDefinition.VerifyNotNull(nameof(queueDefinition));

            QueueDescription createdDescription = await _managementClient.CreateQueueAsync(queueDefinition.ConvertTo(), token);
            _logging.LogTrace($"{nameof(Create)}: QueueName={queueDefinition.QueueName}");

            return createdDescription.ConvertTo();
        }

        public async Task<QueueDefinition> GetDefinition(string queueName, CancellationToken token)
        {
            queueName.VerifyNotEmpty(nameof(queueName));

            QueueDescription queueDescription = await _managementClient.GetQueueAsync(queueName, token);
            _logging.LogTrace($"{nameof(GetDefinition)}: QueueName={queueName}");
            return queueDescription.ConvertTo();
        }

        public async Task Delete(string queueName, CancellationToken token)
        {
            queueName.VerifyNotEmpty(nameof(queueName));

            _logging.LogTrace($"{nameof(Delete)}: QueueName={queueName}");
            await _managementClient.DeleteQueueAsync(queueName, token);
        }

        public async Task<IReadOnlyList<QueueDefinition>> Search(CancellationToken token, string? search = null, int maxSize = 100)
        {
            List<QueueDefinition> list = new List<QueueDefinition>();
            int windowSize = 100;
            int index = 0;

            string regPattern = "^" + Regex.Escape(search ?? string.Empty).Replace("\\*", ".*") + "$";
            Func<string, bool> isMatch = x => Regex.IsMatch(x, regPattern, RegexOptions.IgnoreCase);

            while (list.Count < maxSize)
            {
                IList<QueueDescription> subjects = await _managementClient.GetQueuesAsync(windowSize, index, token);
                if (subjects.Count == 0) break;

                index += subjects.Count;
                list.AddRange(subjects.Where(x => search == null || isMatch(x.Path)).Select(x => x.ConvertTo()));
            }

            return list;
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
