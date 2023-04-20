namespace Toolbox.Security.Sign;

public record SignRequestResponse
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<PrincipleDigest> PrincipleDigests { get; init; } = new List<PrincipleDigest>();

    public IReadOnlyList<string>? Errors { get; init; }
}
