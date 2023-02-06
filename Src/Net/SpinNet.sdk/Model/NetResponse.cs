using System.Net;

namespace SpinNet.sdk.Model;

public record NetResponse
{
    public required HttpStatusCode StatusCode { get; init; }
    public string? Message { get; init; }
}
