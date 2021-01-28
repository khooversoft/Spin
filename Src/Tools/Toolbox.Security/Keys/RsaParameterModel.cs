using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Toolbox.Security
{
    [Serializable]
    public class RsaParameterModel
    {
        public IReadOnlyList<byte>? D { get; set; }

        public IReadOnlyList<byte>? DP { get; set; }

        public IReadOnlyList<byte>? DQ { get; set; }

        public IReadOnlyList<byte>? Exponent { get; set; }

        public IReadOnlyList<byte>? InverseQ { get; set; }

        public IReadOnlyList<byte>? Modulus { get; set; }

        public IReadOnlyList<byte>? P { get; set; }

        public IReadOnlyList<byte>? Q { get; set; }
    }
}
