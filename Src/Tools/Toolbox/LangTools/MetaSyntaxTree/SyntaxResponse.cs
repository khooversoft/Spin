using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.LangTools;

public record SyntaxResponse
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public SyntaxTree SyntaxTree { get; init; } = null!;
}
