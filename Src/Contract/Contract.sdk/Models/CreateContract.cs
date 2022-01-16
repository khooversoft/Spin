using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.sdk.Models
{
    public record CreateContract
    {
        public string ContractId { get; init; } = null!;

        public string Owner { get; init; } = null!;
    }
}
