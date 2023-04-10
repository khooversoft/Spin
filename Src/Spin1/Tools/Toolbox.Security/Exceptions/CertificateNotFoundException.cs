using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Security
{
    [Serializable]
    public class CertificateNotFoundException : Exception
    {
        public CertificateNotFoundException() { }

        public CertificateNotFoundException(string message) : base(message) { }

        public CertificateNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected CertificateNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
