using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.sdk.Models
{
    public record BlkRecord : BlkBase
    {
        public string TrxType { get; init; } = null!;

        public double Value { get; init; }

        public string? Note { get; init; }
    }
}
