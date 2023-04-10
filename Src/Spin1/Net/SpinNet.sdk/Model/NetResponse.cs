using System.Net;

namespace SpinNet.sdk.Model;

public record NetResponse
{
    public required HttpStatusCode StatusCode { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<Payload> Payloads { get; init; } = Array.Empty<Payload>();
    public IReadOnlyList<KeyValuePair<string, string>> Headers { get; init; } = new List<KeyValuePair<string, string>>();
}
