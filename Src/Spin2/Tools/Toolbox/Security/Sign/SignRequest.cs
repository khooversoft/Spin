using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Security.Sign;

public sealed record SignRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string PrincipalId { get; init; } = null!;
    public string MessageDigest { get; init; } = null!;
}


public static class SignRequestValidator
{
    public static IValidator<SignRequest> Validator { get; } = new Validator<SignRequest>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .Build();

    public static Option Validate(this SignRequest request) => Validator.Validate(request).ToOptionStatus();
}

