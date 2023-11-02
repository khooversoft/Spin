using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.LangTools;

public record LangResult
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public string RawData { get; init; } = null!;
    public LangNodes? LangNodes { get; init; }
    public IReadOnlyList<LangTrace> Traces { get; init; } = null!;

    public bool IsError() => StatusCode.IsError();
    public bool IsOk() => StatusCode.IsOk();
}
