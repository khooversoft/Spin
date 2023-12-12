using System.ComponentModel.DataAnnotations;

namespace SpinPortal.Pages.PrincipalKey;

public class PrincipalKeyEditModel
{
    [Required, StringLength(50)] public string KeyId { get; set; } = null!;
    [Required, StringLength(100)] public string OwnerId { get; set; } = null!;
    [Required, StringLength(100)] public string Name { get; set; } = null!;
    public bool PrivateKeyExist { get; set; }
}


public static class PrincipalKeyModelExtensions
{
    public static PrincipalKeyEditModel ConvertTo(this PrincipalKeyModel subject) => new PrincipalKeyEditModel
    {
        KeyId = subject.KeyId,
        OwnerId = subject.PrincipalId,
        Name = subject.Name,
        PrivateKeyExist = subject.PrivateKeyExist,
    };

    public static PrincipalKeyRequest ConvertTo(this PrincipalKeyEditModel subject) => new PrincipalKeyRequest
    {
        KeyId = subject.KeyId,
        OwnerId = subject.OwnerId,
        Name = subject.Name,
    };
}