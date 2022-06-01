using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Tools;

namespace Toolbox.Security
{
    /// <summary>
    /// Local certificate key for certificate stored in Windows certificate store
    /// </summary>
    public class LocalCertificateKey
    {
        public LocalCertificateKey(StoreLocation storeLocation, StoreName storeName, string thumbprint, bool requirePrivateKey)
        {
            thumbprint.NotNull();

            StoreLocation = storeLocation;
            StoreName = storeName;
            Thumbprint = thumbprint;
            RequirePrivateKey = requirePrivateKey;
        }

        public StoreLocation StoreLocation { get; }

        public StoreName StoreName { get; }

        public string Thumbprint { get; }

        public bool RequirePrivateKey { get; }

        public override string ToString()
        {
            var list = new List<string>
            {
                StoreLocation.ToString(),
                StoreName.ToString(),
                Thumbprint,
            };

            return "/" + string.Join("/", list);
        }
    }
}