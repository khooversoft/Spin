using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public class DataLakeStoreFactory : IEnumerable<DataLakeNamespace>, IDataLakeStoreFactory
    {
        protected readonly ILoggerFactory _loggerFactory;
        private readonly ConcurrentDictionary<string, DataLakeNamespace> _namespace = new ConcurrentDictionary<string, DataLakeNamespace>(StringComparer.OrdinalIgnoreCase);

        public DataLakeStoreFactory(ILoggerFactory loggerFactory)
        {
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _loggerFactory = loggerFactory;
        }

        public DataLakeStoreFactory(DataLakeNamespaceOption dataLakeNamespaceOption, ILoggerFactory loggerFactory)
            : this(loggerFactory)
        {
            dataLakeNamespaceOption.VerifyNotNull(nameof(dataLakeNamespaceOption));

            Add(dataLakeNamespaceOption.Namespaces.Values.ToArray());
        }

        public DataLakeStoreFactory Add(params DataLakeNamespace[] dataLakeNamespaces) => this.Action(_ => dataLakeNamespaces.ForEach(x => _namespace[x.Namespace] = x));

        public IDataLakeStore? Create(string nameSpace)
        {
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            if (!_namespace.TryGetValue(nameSpace, out DataLakeNamespace? subject)) return null;

            return new DataLakeStore(subject.Store, _loggerFactory.CreateLogger<DataLakeStore>());
        }

        public IEnumerator<DataLakeNamespace> GetEnumerator() => _namespace.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _namespace.Values.GetEnumerator();

        public bool Remove(string nameSpace) => _namespace.Remove(nameSpace, out DataLakeNamespace? _);

        public bool TryGetValue(string nameSpace, [MaybeNullWhen(false)] out DataLakeNamespace value) => _namespace.TryGetValue(nameSpace, out value);
    }
}