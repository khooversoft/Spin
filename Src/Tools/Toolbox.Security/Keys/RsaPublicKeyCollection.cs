using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security.Keys
{
    public class RsaPublicKeyCollection
    {
        private IDictionary<string, RsaPublicKey> _collection = new ConcurrentDictionary<string, RsaPublicKey>(StringComparer.OrdinalIgnoreCase);

        public RsaPublicKeyCollection() { }

        public RsaPublicKeyCollection(RsaPublicKeyCollection subject)
        {
            subject.NotNull(nameof(subject));

            subject._collection
                .ForEach(x => Add(x.Key, x.Value));
        }

        public RsaPublicKeyCollection Add(string kid, RsaPublicKey rsaPublicKey) => this.Action(x => x._collection.Add(kid, rsaPublicKey));

        public RsaPublicKeyCollection Clear() => this.Action(x => x._collection.Clear());

        public RsaPublicKey Get(string kid) => _collection[kid];

        public bool TryGetValue(string kid, out RsaPublicKey? rsaPublicKey) => _collection.TryGetValue(kid, out rsaPublicKey);

        public RsaPublicKeyCollection Remove(string kid) => this.Action(x => x._collection.Remove(kid));
    }
}
