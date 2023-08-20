using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.User;

public record SignRequest
{
    public string PrincipalId { get; set; } = null!;
    public string MessageDigest { get; init; } = null!;
}

public static class SignRequestValidator
{
    public static IValidator<SignRequest> Validator { get; } = new Validator<SignRequest>()
        .RuleFor(x => x.PrincipalId).NotEmpty().ValidPrincipalId()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .Build();
}
