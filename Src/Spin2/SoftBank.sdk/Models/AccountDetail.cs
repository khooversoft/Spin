using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Models;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record AccountDetail
{
    [Id(0)] public string ObjectId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(4)] public IReadOnlyList<AccessRight> AccessRights { get; init; } = Array.Empty<AccessRight>();

    public bool Equals(AccountDetail? obj) => obj is AccountDetail document &&
        ObjectId == document.ObjectId &&
        OwnerId == document.OwnerId &&
        Name == document.Name &&
        CreatedDate == document.CreatedDate &&
        AccessRights.SequenceEqual(document.AccessRights);

    public override int GetHashCode() => HashCode.Combine(ObjectId, OwnerId, Name, CreatedDate);
}


public static class AccountDetailValidator
{
    public static IValidator<AccountDetail> Validator { get; } = new Validator<AccountDetail>()
        .RuleFor(x => x.ObjectId).ValidObjectId()
        .RuleFor(x => x.OwnerId).ValidName()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.Name).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this AccountDetail subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static bool IsValid(this AccountDetail subject, ScopeContextLocation location) => subject.Validate(location).IsValid;

    public static Option CanAccess(this AccountDetail subject, string privilege, PrincipalId principalId) => subject.AccessRights
        .CanAccess(privilege, principalId);
}