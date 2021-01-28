using System.Collections.Generic;

namespace Toolbox.Security
{
    public interface IPrincipleSignatureContainer : IEnumerable<PrincipleSignature>
    {
        void Clear();

        PrincipleSignature? Get(string issuer);

        IPrincipleSignatureContainer Add(params PrincipleSignature[] principleSignature);
    }
}