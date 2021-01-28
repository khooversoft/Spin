using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Security
{

    [Serializable]
    public class SecurityException : Exception
    {
        public SecurityException() { }
        public SecurityException(string message) : base(message) { }
        public SecurityException(string message, Exception inner) : base(message, inner) { }
        protected SecurityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
