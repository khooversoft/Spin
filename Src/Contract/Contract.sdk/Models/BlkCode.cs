using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.sdk.Models;

public record BlkCode : BlkBase
{
    public string Language { get; init; } = "C#";

    public string Framework { get; init; } = ".net6.0";

    public IReadOnlyList<string> Lines { get; init; } = new List<string>();
}
