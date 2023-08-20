using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

public record ValidateRequest
{
    public string JwtSignature { get; init; } = null!;
    public string MessageDigest { get; init; } = null!;
}


public static class ValidateRequestValidator
{
    public static IValidator<ValidateRequest> Validator { get; } = new Validator<ValidateRequest>()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .Build();

    public static Option Validate(this ValidateRequest subject) => Validator.Validate(subject).ToOptionStatus();
}
