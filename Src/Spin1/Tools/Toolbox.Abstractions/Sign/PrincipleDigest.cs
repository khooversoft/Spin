namespace Toolbox.Sign;

public record PrincipleDigest
{
    public string Key { get; init; } = Guid.NewGuid().ToString();

    public string PrincipleId { get; init; } = null!;

    public string Digest { get; init; } = null!;

    public string? JwtSignature { get; init; }
}
