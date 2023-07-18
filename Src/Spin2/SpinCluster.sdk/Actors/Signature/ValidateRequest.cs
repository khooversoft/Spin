using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

public record ValidateRequest
{
    public string JwtSignature { get; init; } = null!;
    public string Digest { get; init; } = null!;
}


public static class ValidateRequestValidator
{
    public static IValidator<ValidateRequest> Validator { get; } = new Validator<ValidateRequest>()
        .RuleFor(x => x.Digest).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this ValidateRequest signRequest, ScopeContextLocation location) => Validator
        .Validate(signRequest)
        .LogResult(location);
}
