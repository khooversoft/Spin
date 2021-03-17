using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.sdk.Models
{
    public record Signature
    {
        public string SignatureId { get; set; } = null!;

        public string Key { get; set; } = null!;
    }
}
