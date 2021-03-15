using System.Collections.Generic;

namespace Toolbox.Security
{
    public interface IPrincipleSignatureCollection : IEnumerable<IPrincipleSignature>
    {
        public IPrincipleSignature this[string issuer] { get; }

        IPrincipleSignatureCollection Add(params IPrincipleSignature[] principleSignature);

        void Clear();

        IPrincipleSignature? Get(string issuer);

        IPrincipleSignature GetRequired(string issuer);
    }
}