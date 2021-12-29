using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Directory.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk
{
    public class Database
    {
        private readonly ConcurrentDictionary<string, StorageRecord> _storage = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, QueueRecord> _queue = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ServiceRecord> _service = new(StringComparer.OrdinalIgnoreCase);

        public Database(string environment)
        {
            environment.VerifyNotNull(nameof(environment));

            Environment = environment;
        }

        public IReadOnlyDictionary<string, StorageRecord> Storage => _storage;
        public IReadOnlyDictionary<string, QueueRecord> Queue => _queue;
        public IReadOnlyDictionary<string, ServiceRecord> Service => _service;

        public string Environment { get; }

        public void Add(StorageRecord record) => _storage[record.StorageId] = record.Verify();
        public void Add(QueueRecord record) => _queue[record.Channel] = record.Verify();
        public void Add(ServiceRecord record) => _service[record.ServiceId] = record.Verify();

        public Database Load(EnvironmentModel model)
        {
            model.Storage.ForEach(x => Add(x));
            model.Queue.ForEach(x => Add(x));
            model.Service.ForEach(x => Add(x));

            return this;
        }
    }
}
