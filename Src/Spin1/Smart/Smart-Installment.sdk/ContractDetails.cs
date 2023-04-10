using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Installment.sdk;

public record ContractDetails
{
    public int NumPayments { get; init; }
    public decimal Principal { get; init; }
    public decimal Payment { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? CompletedDate { get; init; }
}
