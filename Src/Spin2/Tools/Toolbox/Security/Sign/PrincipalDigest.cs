namespace Toolbox.Security.Sign;

public record PrincipalDigest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string PrincipleId { get; init; } = null!;
    public string MessageDigest { get; init; } = null!;
    public string JwtSignature { get; init; } = null!;
}
