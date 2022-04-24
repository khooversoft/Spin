using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Abstractions.Model;

public record TrxBalance
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string AccountId { get; init; } = null!;

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public decimal Balance { get; init; }
}
