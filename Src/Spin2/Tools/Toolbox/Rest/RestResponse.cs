using System.Net;
using Azure;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Rest;

public record RestResponse
{
    public HttpStatusCode StatusCode { get; init; }
    public string? Content { get; init; }
}
