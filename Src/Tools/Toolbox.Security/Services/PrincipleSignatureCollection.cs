using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security
{
    public class PrincipleSignatureCollection : IPrincipleSignatureCollection, IEnumerable<IPrincipleSignature>
    {
        private readonly ConcurrentDictionary<string, IPrincipleSignature> _registration = new ConcurrentDictionary<string, IPrincipleSignature>(StringComparer.OrdinalIgnoreCase);

        public IPrincipleSignature this[string issuer] => GetRequired(issuer);

        public void Clear() => _registration.Clear();

        public IPrincipleSignatureCollection Add(params IPrincipleSignature[] principleSignature)
        {
            principleSignature
                .ForEach(x => _registration[x.Issuer] = x);

            return this;
        }

        public IPrincipleSignature? Get(string issuer)
        {
            if (!_registration.TryGetValue(issuer, out IPrincipleSignature? principleSignature))
            {
                return null;
            }

            return principleSignature;
        }

        public IPrincipleSignature GetRequired(string issuer) => Get(issuer).VerifyNotNull($"Issuer {issuer} not found");

        public IEnumerator<IPrincipleSignature> GetEnumerator() => _registration.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _registration.Values.GetEnumerator();
    }
}