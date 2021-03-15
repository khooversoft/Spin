using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security.Keys;

namespace Toolbox.Security.Services
{
    public class KeyServiceBuilder
    {
        public CertificateCollection Certificates { get; } = new CertificateCollection();

        public RsaPublicKeyCollection PublicKeys { get; } = new RsaPublicKeyCollection();

        public KeyServiceBuilder Add(string kid, X509Certificate2 certificate) => this.Action(x => x.Certificates.Add(kid, certificate));

        public KeyServiceBuilder Add(string kid, RsaPublicKey rsaPublicKey) => this.Action(x => x.PublicKeys.Add(kid, rsaPublicKey));

        public IKeyService Build() => new KeyService(Certificates, PublicKeys);
    }
}
