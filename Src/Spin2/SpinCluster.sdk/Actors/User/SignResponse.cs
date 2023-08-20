using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.User;

public record SignResponse
{
    public string Kid { get; set; } = null!;
    public string MessageDigest { get; init; } = null!;
    public string JwtSignature { get; init; } = null!;
}

public static class SignResponseValidator
{
    public static IValidator<SignResponse> Validator { get; } = new Validator<SignResponse>()
        .RuleFor(x => x.Kid).NotEmpty().ValidObjectId()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .Build();
}
