using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallmentContract.Provider.Models;

public record BalanceRecord
{
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public required decimal Amount { get; init; }
}
