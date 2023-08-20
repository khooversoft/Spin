using Toolbox.Tools.Validation;

namespace Toolbox.Security.Sign;

public sealed record SignResponse
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string ReferenceId { get; init; } = null!;
    public string PrincipleId { get; init; } = null!;
    public string MessageDigest { get; init; } = null!;
    public string JwtSignature { get; init; } = null!;
}


public static class SignResponseValidator
{
    public static IValidator<SignResponse> Validator { get; } = new Validator<SignResponse>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.ReferenceId).NotEmpty()
        .RuleFor(x => x.PrincipleId).NotEmpty()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .Build();
}