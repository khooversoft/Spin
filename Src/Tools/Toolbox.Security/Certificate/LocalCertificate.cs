using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Tools;

namespace Toolbox.Security
{
    /// <summary>
    /// Local certificate specifies X509 certificates stored in Windows certificate store.
    /// Once the certificate has been located and loaded, it is cached.
    /// </summary>
    [DebuggerDisplay("StoreName={LocalCertificateKey.StoreName}, StoreLocation={LocalCertificateKey.StoreLocation}, Thumbprint={LocalCertificateKey.Thumbprint}")]
    public class LocalCertificate
    {
        private readonly object _lock = new object();
        private readonly CacheObject<X509Certificate2> _cachedCertificate = new CacheObject<X509Certificate2>(TimeSpan.FromDays(1));
        private readonly ILogger<LocalCertificate> _logger;

        public LocalCertificate(LocalCertificateKey key, ILogger<LocalCertificate> logger)
        {
            key.VerifyNotNull(nameof(key));

            LocalCertificateKey = key;
            _logger = logger;
        }

        public LocalCertificate(StoreLocation storeLocation, StoreName storeName, string thumbprint, bool requirePrivateKey, ILogger<LocalCertificate> logger)
        {
            LocalCertificateKey = new LocalCertificateKey(storeLocation, storeName, thumbprint, requirePrivateKey);
            _logger = logger;
        }

        /// <summary>
        /// Local certificate key
        /// </summary>
        public LocalCertificateKey LocalCertificateKey { get; }

        /// <summary>
        /// Find certificate by thumbprint.  Certificates that have expired will not
        /// be returned and if "throwOnNotFound" is specified, an exception will be
        /// thrown.
        /// </summary>
        /// <param name="tag">tag</param>
        /// <param name="context">work context</param>
        /// <param name="throwOnNotFound">if true, throw exception if not found</param>
        /// <exception cref="ProgramExitException">Certificate is not found</exception>
        /// <returns>X509 certificate</returns>
        /// <exception cref="CertificateNotFoundException">when certificate valid certificate was not found</exception>
        public X509Certificate2? GetCertificate(bool? throwOnNotFound = null)
        {
            throwOnNotFound ??= LocalCertificateKey.RequirePrivateKey;

            lock (_lock)
            {
                if (_cachedCertificate.TryGetValue(out X509Certificate2? certificate))
                {
                    return certificate;
                }

                using (X509Store store = new X509Store(LocalCertificateKey.StoreName, LocalCertificateKey.StoreLocation))
                {
                    _logger.LogTrace($"Looking for certificate for {this}");

                    try
                    {
                        store.Open(OpenFlags.ReadOnly);
                        X509Certificate2Collection certificateList = store.Certificates.Find(X509FindType.FindByThumbprint, LocalCertificateKey.Thumbprint, validOnly: false);

                        if (certificateList?.Count != 0)
                        {
                            X509Certificate2? cert = certificateList!
                                    .OfType<X509Certificate2>()
                                    .Where(x => !LocalCertificateKey.RequirePrivateKey || x.HasPrivateKey)
                                    .Where(x => DateTime.Now <= x.NotAfter)
                                    .FirstOrDefault();

                            if (cert == null)
                            {
                                _logger.LogTrace($"Certificate Not found for {this}");
                                if (throwOnNotFound == true) throw new CertificateNotFoundException($"Certificate not found: {LocalCertificateKey.ToString()}");
                                return null;
                            }

                            _cachedCertificate.Set(cert);
                            return cert;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Exception: {ex}");
                        _cachedCertificate.Clear();
                    }
                }

                _cachedCertificate.Clear();
                return null;
            }
        }
    }
}
