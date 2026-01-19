using System.Net;
using Microsoft.Extensions.Logging;

namespace Toolbox.Rest;

public readonly record struct RestResponse
{
    public required HttpStatusCode StatusCode { get; init; }
    public string? Content { get; init; }
    public required ILogger Logger { get; init; }
}
