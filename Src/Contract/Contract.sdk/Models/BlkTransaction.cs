using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.sdk.Models
{
    public record BlkTransaction : BlkBase
    {
        public IReadOnlyList<BlkRecord> Records { get; init; } = new List<BlkRecord>();
    }
}
