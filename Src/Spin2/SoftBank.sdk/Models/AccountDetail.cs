using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public record AccountDetail
{
    [Id(0)] public string ObjectId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
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
}