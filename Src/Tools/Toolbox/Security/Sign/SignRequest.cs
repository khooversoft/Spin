using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Security;

public sealed record SignRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string PrincipalId { get; init; } = null!;
    public string MessageDigest { get; init; } = null!;

    public static IValidator<SignRequest> Validator { get; } = new Validator<SignRequest>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .Build();
}


public static class SignRequestValidator
{
    public static Option Validate(this SignRequest subject) => SignRequest.Validator.Validate(subject).ToOptionStatus();
}

