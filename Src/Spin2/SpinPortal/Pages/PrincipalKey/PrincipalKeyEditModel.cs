using System.ComponentModel.DataAnnotations;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Signature;

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
        OwnerId = subject.OwnerId,
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