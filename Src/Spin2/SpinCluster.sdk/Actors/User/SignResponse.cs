using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public record SignResponse
{
    [Id(0)] public string Kid { get; set; } = null!;
    [Id(1)] public string MessageDigest { get; init; } = null!;
    [Id(2)] public string JwtSignature { get; init; } = null!;
}

public static class SignResponseValidator
{
    public static IValidator<SignResponse> Validator { get; } = new Validator<SignResponse>()
        .RuleFor(x => x.Kid).NotEmpty().ValidObjectId()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .Build();
}
