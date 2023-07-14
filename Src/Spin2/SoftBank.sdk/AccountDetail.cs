using Toolbox.Block;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk;

public record AccountDetail
{
    public string ObjectId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}

public static class AccountDetailValidator
{
    public static IValidator<AccountDetail> Validator { get; } = new Validator<AccountDetail>()
        .RuleFor(x => x.ObjectId).ValidObjectId()
        .RuleFor(x => x.OwnerId).NotEmpty()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();

    public static ValidatorResult Validate(this AccountDetail subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static bool IsValid(this AccountDetail subject, ScopeContextLocation location) => Validate(subject, location).IsValid;
}