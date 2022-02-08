using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.sdk.Model;

public record TrxBatch<T>
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<T> Items { get; init; } = new List<T>();
}
