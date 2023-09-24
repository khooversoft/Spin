using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loan_smartc_v1.Models;

internal sealed record CreateContractModel
{
    public string ContractId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
}
