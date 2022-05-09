using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security.Keys
{
    public class CertificateCollection
    {
        private IDictionary<string, X509Certificate2> _collection = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

        public CertificateCollection() { }

        public CertificateCollection(CertificateCollection subject)
        {
            subject.NotNull(nameof(subject));

            subject._collection
                .ForEach(x => Add(x.Key, x.Value));
        }

        public CertificateCollection Add(string kid, X509Certificate2 certificate) => this.Action(x => x._collection.Add(kid, certificate));

        public CertificateCollection Clear() => this.Action(x => x._collection.Clear());

        public X509Certificate2 Get(string kid) => _collection[kid];

        public bool TryGetValue(string kid, out X509Certificate2? certificate) => _collection.TryGetValue(kid, out certificate);

        public CertificateCollection Remove(string kid) => this.Action(x => x._collection.Remove(kid));
    }
}
