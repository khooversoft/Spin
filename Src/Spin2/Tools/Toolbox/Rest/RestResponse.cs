using System.Net;
using Toolbox.Types;

namespace Toolbox.Rest;

public readonly record struct RestResponse
{
    public required HttpStatusCode StatusCode { get; init; }
    public string? Content { get; init; }
    public required ScopeContext Context { get; init; }
}
