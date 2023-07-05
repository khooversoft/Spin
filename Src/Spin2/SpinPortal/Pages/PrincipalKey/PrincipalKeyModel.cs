namespace SpinPortal.Pages.PrincipalKey;

public class PrincipalKeyModel
{
    public string KeyId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public bool PublicKeyExist { get; init; }
    public bool PrivateKeyExist { get; init; }
}
