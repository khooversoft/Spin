using System.Net;

namespace Toolbox.Rest;

public record RestResponse
{
    public HttpStatusCode StatusCode { get; init; }
    public string? Content { get; init; }
}
