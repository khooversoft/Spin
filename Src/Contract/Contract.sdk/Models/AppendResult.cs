using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Contract.sdk.Models;

public record AppendResult : BatchResult<AppendState>
{
    public override string ToString() => $"AppendResult={{{Items.Select(x => x.ToString()).Join(",")}}}";

    public int SuccessCount => Items.Count(x => x.Success);
    public int ErrorCount => Items.Count(x => !x.Success);
    public bool HasError => ErrorCount > 0;
}

public record AppendState
{
    public AppendState(bool success, string documentId)
    {
        Success = success;
        DocumentId = documentId;
    }

    public bool Success { get; }
    public string DocumentId { get; }

    public override string ToString() => $"Success={Success}, DocumentId={DocumentId}";
}
