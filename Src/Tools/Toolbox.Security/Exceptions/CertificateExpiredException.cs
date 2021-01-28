using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Security
{
    [Serializable]
    public class CertificateExpiredException : Exception
    {
        public CertificateExpiredException() { }

        public CertificateExpiredException(string message) : base(message) { }

        public CertificateExpiredException(string message, Exception inner) : base(message, inner) { }

        protected CertificateExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}