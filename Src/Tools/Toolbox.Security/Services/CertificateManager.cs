//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using Toolbox.Tools;

//namespace Toolbox.Security
//{
//    /// <summary>
//    /// Manage a collection of local certificates
//    /// </summary>
//    public class CertificateManager : ICertificateManager
//    {
//        private readonly ConcurrentDictionary<string, LocalCertificate> _registration = new ConcurrentDictionary<string, LocalCertificate>(StringComparer.OrdinalIgnoreCase);

//        public CertificateManager()
//        {
//        }

//        public void Clear()
//        {
//            _registration.Clear();
//        }

//        public void Set(LocalCertificate certificate)
//        {
//            certificate.VerifyNotNull(nameof(certificate));

//            _registration[certificate.LocalCertificateKey.Thumbprint] = certificate;
//        }

//        public LocalCertificate? Get(string thumbprint)
//        {
//            if (!_registration.TryGetValue(thumbprint, out LocalCertificate? spec))
//            {
//                return null;
//            }

//            return spec;
//        }

//        public IEnumerator<LocalCertificate> LocalCertificateItems()
//        {
//            return _registration.Values.GetEnumerator();
//        }
//    }
//}