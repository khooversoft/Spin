using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record PrincipalKeyCreateModel
{
    [Id(0)] public string PrincipalKeyId { get; init; } = null!;
    [Id(1)] public string KeyId { get; init; } = null!;
    [Id(2)] public string PrincipalId { get; init; } = null!;
    [Id(3)] public string Name { get; init; } = null!;
    [Id(4)] public string PrincipalPrivateKeyId { get; init; } = null!;

    public bool Equals(PrincipalKeyCreateModel? obj) => obj is PrincipalKeyCreateModel document &&
        KeyId == document.KeyId &&
        PrincipalId == document.PrincipalId &&
        Name == document.Name &&
        PrincipalPrivateKeyId == document.PrincipalPrivateKeyId;

    public override int GetHashCode() => HashCode.Combine(KeyId, PrincipalId, Name);

    public static IValidator<PrincipalKeyCreateModel> Validator { get; } = new Validator<PrincipalKeyCreateModel>()
        .RuleFor(x => x.KeyId).ValidResourceId(ResourceType.Owned)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.PrincipalPrivateKeyId).ValidResourceId(ResourceType.Owned)
        .Build();
}


public static class PrincipalKeyCreateModelValidator
{
    public static Option Validate(this PrincipalKeyCreateModel subject) => PrincipalKeyCreateModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this PrincipalKeyCreateModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}

